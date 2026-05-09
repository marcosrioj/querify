using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Querify.Common.Infrastructure.Storage.Abstractions;
using Querify.Models.QnA.Enums;
using Querify.Models.QnA.Events;
using Querify.QnA.Common.Domain.BusinessRules.Sources;
using Querify.QnA.Common.Domain.Options;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Querify.QnA.Worker.Business.Source.Abstractions;

namespace Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSource;

public sealed class VerifyUploadedSourceCommandHandler(
    QnADbContext dbContext,
    IObjectStorage objectStorage,
    IUploadThreatScanner threatScanner,
    IOptions<SourceUploadOptions> uploadOptions,
    ISourceUploadStatusChangedEventPublisher statusChangedEventPublisher,
    ILogger<VerifyUploadedSourceCommandHandler> logger)
    : IRequestHandler<VerifyUploadedSourceCommand, Guid>
{
    private const string SystemUser = "system:qna-worker";
    private const int PrefixByteLimit = 512;

    public async Task<Guid> Handle(VerifyUploadedSourceCommand request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = await dbContext.Sources
            .SingleOrDefaultAsync(source => source.TenantId == request.TenantId &&
                                            source.Id == request.SourceId, cancellationToken);

        if (entity is null)
        {
            logger.LogWarning(
                "Skipping source upload verification because source {SourceId} was not found for tenant {TenantId}.",
                request.SourceId,
                request.TenantId);
            return request.SourceId;
        }

        if (entity.UploadStatus is not SourceUploadStatus.Uploaded)
        {
            logger.LogInformation(
                "Skipping source upload verification for source {SourceId} because status is {UploadStatus}.",
                entity.Id,
                entity.UploadStatus);

            if (IsTerminalUploadStatus(entity.UploadStatus))
            {
                await PublishStatusChangedAsync(entity, reason: null, cancellationToken);
            }

            return entity.Id;
        }

        if (entity.StorageKey != request.StorageKey || !SourceStorageKey.IsStagingKey(request.StorageKey))
        {
            await FailSourceAsync(
                entity,
                request.StorageKey,
                deleteStaging: false,
                "Storage key did not match the uploaded source.",
                cancellationToken);
            return entity.Id;
        }

        var head = await objectStorage.HeadAsync(request.StorageKey, cancellationToken);
        if (head is null)
        {
            await FailSourceAsync(
                entity,
                request.StorageKey,
                deleteStaging: false,
                "Uploaded object was not found in storage.",
                cancellationToken);
            return entity.Id;
        }

        if (head.SizeBytes != entity.SizeBytes || head.SizeBytes > uploadOptions.Value.MaxUploadBytes)
        {
            await FailSourceAsync(
                entity,
                request.StorageKey,
                deleteStaging: true,
                "Uploaded object size does not match the upload intent.",
                cancellationToken);
            return entity.Id;
        }

        var normalizedHeadContentType = SourceRules.NormalizeContentType(head.ContentType);
        var normalizedEntityContentType = SourceRules.NormalizeContentType(entity.MediaType);
        if (normalizedHeadContentType is null ||
            normalizedEntityContentType is null ||
            !StringComparer.OrdinalIgnoreCase.Equals(normalizedHeadContentType, normalizedEntityContentType))
        {
            await FailSourceAsync(
                entity,
                request.StorageKey,
                deleteStaging: true,
                "Uploaded object content type does not match the upload intent.",
                cancellationToken);
            return entity.Id;
        }

        await using (var scanStream = await objectStorage.OpenReadAsync(request.StorageKey, cancellationToken))
        {
            var scanResult = await threatScanner.ScanAsync(scanStream, cancellationToken);
            if (!scanResult.IsSafe)
            {
                await QuarantineSourceAsync(entity, request.StorageKey, scanResult.Reason, cancellationToken);
                return entity.Id;
            }
        }

        var evidence = await ReadObjectEvidenceAsync(request.StorageKey, cancellationToken);
        if (evidence.SizeBytes != head.SizeBytes)
        {
            await FailSourceAsync(
                entity,
                request.StorageKey,
                deleteStaging: true,
                "Uploaded object size changed while verification was running.",
                cancellationToken);
            return entity.Id;
        }

        if (!SourceUploadContentInspector.IsAllowed(
                normalizedHeadContentType,
                evidence.Prefix,
                uploadOptions.Value.AllowedContentTypes))
        {
            await FailSourceAsync(
                entity,
                request.StorageKey,
                deleteStaging: true,
                "Uploaded object contents do not match the allowed source type.",
                cancellationToken);
            return entity.Id;
        }

        var computedChecksum = $"sha256:{evidence.Sha256Hex}";
        if (!ClientChecksumMatches(entity.Checksum, request.StorageKey, computedChecksum))
        {
            logger.LogWarning(
                "Uploaded source {SourceId} failed checksum verification. Expected client checksum {ClientChecksum}; computed {ComputedChecksum}.",
                entity.Id,
                entity.Checksum,
                computedChecksum);
            await FailSourceAsync(
                entity,
                request.StorageKey,
                deleteStaging: true,
                "Uploaded object checksum does not match the provided checksum.",
                cancellationToken);
            return entity.Id;
        }

        var verifiedKey = SourceStorageKey.ToVerifiedKey(request.StorageKey);
        await objectStorage.CopyAsync(request.StorageKey, verifiedKey, cancellationToken);

        var transitioned = await TryMarkVerifiedAsync(
            entity,
            request.StorageKey,
            verifiedKey,
            computedChecksum,
            head.SizeBytes,
            normalizedHeadContentType,
            cancellationToken);
        if (transitioned)
        {
            await TryDeleteStagingObjectAsync(entity.Id, request.StorageKey, cancellationToken);
            await PublishStatusChangedAsync(entity, reason: null, cancellationToken);
        }

        return entity.Id;
    }

    private async Task<ObjectEvidence> ReadObjectEvidenceAsync(string storageKey, CancellationToken cancellationToken)
    {
        await using var stream = await objectStorage.OpenReadAsync(storageKey, cancellationToken);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[81920];
        var prefix = new byte[PrefixByteLimit];
        var prefixLength = 0;
        long totalBytes = 0;

        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken);
            if (read == 0)
            {
                break;
            }

            hash.AppendData(buffer.AsSpan(0, read));
            totalBytes += read;

            if (prefixLength < prefix.Length)
            {
                var bytesToCopy = Math.Min(read, prefix.Length - prefixLength);
                buffer.AsSpan(0, bytesToCopy).CopyTo(prefix.AsSpan(prefixLength));
                prefixLength += bytesToCopy;
            }
        }

        var sha256 = Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
        return new ObjectEvidence(totalBytes, sha256, prefix.AsSpan(0, prefixLength).ToArray());
    }

    private async Task FailSourceAsync(
        Common.Domain.Entities.Source entity,
        string stagingKey,
        bool deleteStaging,
        string reason,
        CancellationToken cancellationToken)
    {
        var transitioned = await TryMarkFailedAsync(entity, cancellationToken);
        if (!transitioned)
        {
            return;
        }

        if (deleteStaging)
        {
            await TryDeleteStagingObjectAsync(entity.Id, stagingKey, cancellationToken);
        }

        await PublishStatusChangedAsync(entity, reason, cancellationToken);
    }

    private async Task QuarantineSourceAsync(
        Common.Domain.Entities.Source entity,
        string stagingKey,
        string? reason,
        CancellationToken cancellationToken)
    {
        var quarantineKey = SourceStorageKey.ToQuarantineKey(stagingKey);
        await objectStorage.CopyAsync(stagingKey, quarantineKey, cancellationToken);

        var transitioned = await TryMarkQuarantinedAsync(entity, quarantineKey, cancellationToken);
        if (!transitioned)
        {
            return;
        }

        await TryDeleteStagingObjectAsync(entity.Id, stagingKey, cancellationToken);

        await PublishStatusChangedAsync(
            entity,
            reason ?? "Scanner returned unsafe verdict.",
            cancellationToken);

        logger.LogWarning(
            "Uploaded source {SourceId} was quarantined. Reason: {Reason}",
            entity.Id,
            reason ?? "Scanner returned unsafe verdict.");
    }

    private static bool ClientChecksumMatches(string? persistedChecksum, string stagingKey, string computedChecksum)
    {
        var normalized = SourceChecksum.NormalizeOptional(persistedChecksum);
        if (normalized is null)
        {
            return false;
        }

        if (SourceChecksum.IsLocatorChecksum(normalized, stagingKey))
        {
            return true;
        }

        return FixedTimeEquals(normalized, computedChecksum);
    }

    private async Task PublishStatusChangedAsync(
        Common.Domain.Entities.Source entity,
        string? reason,
        CancellationToken cancellationToken)
    {
        await statusChangedEventPublisher.PublishAsync(new SourceUploadStatusChangedIntegrationEvent
        {
            EventId = Guid.NewGuid(),
            OccurredAtUtc = DateTime.UtcNow,
            TenantId = entity.TenantId,
            SourceId = entity.Id,
            UploadStatus = entity.UploadStatus,
            StorageKey = entity.StorageKey,
            Checksum = entity.Checksum,
            Reason = reason
        }, cancellationToken);
    }

    private static bool IsTerminalUploadStatus(SourceUploadStatus uploadStatus)
    {
        return uploadStatus is SourceUploadStatus.Verified or
            SourceUploadStatus.Failed or
            SourceUploadStatus.Quarantined;
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length &&
               CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private async Task<bool> TryMarkVerifiedAsync(
        Common.Domain.Entities.Source entity,
        string stagingKey,
        string verifiedKey,
        string computedChecksum,
        long sizeBytes,
        string mediaType,
        CancellationToken cancellationToken)
    {
        var updatedRows = await dbContext.Sources
            .Where(source => source.TenantId == entity.TenantId &&
                             source.Id == entity.Id &&
                             source.UploadStatus == SourceUploadStatus.Uploaded &&
                             source.StorageKey == stagingKey)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(source => source.StorageKey, verifiedKey)
                    .SetProperty(source => source.Locator, verifiedKey)
                    .SetProperty(source => source.Checksum, computedChecksum)
                    .SetProperty(source => source.SizeBytes, sizeBytes)
                    .SetProperty(source => source.MediaType, mediaType)
                    .SetProperty(source => source.UploadStatus, SourceUploadStatus.Verified)
                    .SetProperty(source => source.UpdatedBy, SystemUser),
                cancellationToken);

        if (updatedRows == 0)
        {
            logger.LogInformation(
                "Skipping verified transition for source {SourceId} because another worker already changed it.",
                entity.Id);
            return false;
        }

        entity.StorageKey = verifiedKey;
        entity.Locator = verifiedKey;
        entity.Checksum = computedChecksum;
        entity.SizeBytes = sizeBytes;
        entity.MediaType = mediaType;
        entity.UploadStatus = SourceUploadStatus.Verified;
        entity.UpdatedBy = SystemUser;
        return true;
    }

    private async Task<bool> TryMarkFailedAsync(
        Common.Domain.Entities.Source entity,
        CancellationToken cancellationToken)
    {
        var currentStorageKey = entity.StorageKey;
        var currentChecksum = SourceChecksum.NormalizeOptional(entity.Checksum) ??
                              SourceChecksum.FromLocator(currentStorageKey ?? entity.Locator);
        var updatedRows = await dbContext.Sources
            .Where(source => source.TenantId == entity.TenantId &&
                             source.Id == entity.Id &&
                             source.UploadStatus == SourceUploadStatus.Uploaded &&
                             source.StorageKey == currentStorageKey)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(source => source.UploadStatus, SourceUploadStatus.Failed)
                    .SetProperty(source => source.Checksum, currentChecksum)
                    .SetProperty(source => source.UpdatedBy, SystemUser),
                cancellationToken);

        if (updatedRows == 0)
        {
            logger.LogInformation(
                "Skipping failed transition for source {SourceId} because another worker already changed it.",
                entity.Id);
            return false;
        }

        entity.UploadStatus = SourceUploadStatus.Failed;
        entity.Checksum = currentChecksum;
        entity.UpdatedBy = SystemUser;
        return true;
    }

    private async Task<bool> TryMarkQuarantinedAsync(
        Common.Domain.Entities.Source entity,
        string quarantineKey,
        CancellationToken cancellationToken)
    {
        var currentStorageKey = entity.StorageKey;
        var currentChecksum = SourceChecksum.NormalizeOptional(entity.Checksum) ??
                              SourceChecksum.FromLocator(currentStorageKey ?? entity.Locator);
        var updatedRows = await dbContext.Sources
            .Where(source => source.TenantId == entity.TenantId &&
                             source.Id == entity.Id &&
                             source.UploadStatus == SourceUploadStatus.Uploaded &&
                             source.StorageKey == currentStorageKey)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(source => source.StorageKey, quarantineKey)
                    .SetProperty(source => source.Locator, quarantineKey)
                    .SetProperty(source => source.UploadStatus, SourceUploadStatus.Quarantined)
                    .SetProperty(source => source.Checksum, currentChecksum)
                    .SetProperty(source => source.UpdatedBy, SystemUser),
                cancellationToken);

        if (updatedRows == 0)
        {
            logger.LogInformation(
                "Skipping quarantined transition for source {SourceId} because another worker already changed it.",
                entity.Id);
            return false;
        }

        entity.StorageKey = quarantineKey;
        entity.Locator = quarantineKey;
        entity.UploadStatus = SourceUploadStatus.Quarantined;
        entity.Checksum = currentChecksum;
        entity.UpdatedBy = SystemUser;
        return true;
    }

    private async Task TryDeleteStagingObjectAsync(
        Guid sourceId,
        string stagingKey,
        CancellationToken cancellationToken)
    {
        try
        {
            await objectStorage.DeleteAsync(stagingKey, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Source {SourceId} transitioned but staging object {StorageKey} could not be deleted.",
                sourceId,
                stagingKey);
        }
    }

    private sealed record ObjectEvidence(long SizeBytes, string Sha256Hex, byte[] Prefix);
}
