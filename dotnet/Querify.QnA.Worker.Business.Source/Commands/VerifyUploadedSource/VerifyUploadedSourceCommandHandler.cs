using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Querify.Common.Infrastructure.Storage.Abstractions;
using Querify.Models.QnA.Enums;
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
            return entity.Id;
        }

        if (entity.StorageKey != request.StorageKey || !SourceStorageKey.IsStagingKey(request.StorageKey))
        {
            await FailSourceAsync(entity, request.StorageKey, deleteStaging: false, cancellationToken);
            return entity.Id;
        }

        var head = await objectStorage.HeadAsync(request.StorageKey, cancellationToken);
        if (head is null)
        {
            await FailSourceAsync(entity, request.StorageKey, deleteStaging: false, cancellationToken);
            return entity.Id;
        }

        if (head.SizeBytes != entity.SizeBytes || head.SizeBytes > uploadOptions.Value.MaxUploadBytes)
        {
            await FailSourceAsync(entity, request.StorageKey, deleteStaging: true, cancellationToken);
            return entity.Id;
        }

        var normalizedHeadContentType = SourceRules.NormalizeContentType(head.ContentType);
        var normalizedEntityContentType = SourceRules.NormalizeContentType(entity.MediaType);
        if (normalizedHeadContentType is null ||
            normalizedEntityContentType is null ||
            !StringComparer.OrdinalIgnoreCase.Equals(normalizedHeadContentType, normalizedEntityContentType))
        {
            await FailSourceAsync(entity, request.StorageKey, deleteStaging: true, cancellationToken);
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
            await FailSourceAsync(entity, request.StorageKey, deleteStaging: true, cancellationToken);
            return entity.Id;
        }

        if (!SourceUploadContentInspector.IsAllowed(entity.Kind, normalizedHeadContentType, evidence.Prefix))
        {
            await FailSourceAsync(entity, request.StorageKey, deleteStaging: true, cancellationToken);
            return entity.Id;
        }

        var computedChecksum = $"sha256:{evidence.Sha256Hex}";
        if (!ClientChecksumMatches(request.ClientChecksum, computedChecksum))
        {
            logger.LogWarning(
                "Uploaded source {SourceId} failed checksum verification. Expected client checksum {ClientChecksum}; computed {ComputedChecksum}.",
                entity.Id,
                request.ClientChecksum,
                computedChecksum);
            await FailSourceAsync(entity, request.StorageKey, deleteStaging: true, cancellationToken);
            return entity.Id;
        }

        var verifiedKey = SourceStorageKey.ToVerifiedKey(request.StorageKey);
        await objectStorage.CopyAsync(request.StorageKey, verifiedKey, cancellationToken);
        await objectStorage.DeleteAsync(request.StorageKey, cancellationToken);

        entity.StorageKey = verifiedKey;
        entity.Locator = verifiedKey;
        entity.Checksum = computedChecksum;
        entity.SizeBytes = head.SizeBytes;
        entity.MediaType = normalizedHeadContentType;
        entity.LastVerifiedAtUtc = DateTime.UtcNow;
        entity.UploadStatus = SourceUploadStatus.Verified;
        entity.UpdatedBy = SystemUser;
        await dbContext.SaveChangesAsync(cancellationToken);

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
        CancellationToken cancellationToken)
    {
        if (deleteStaging)
        {
            await objectStorage.DeleteAsync(stagingKey, cancellationToken);
        }

        entity.UploadStatus = SourceUploadStatus.Failed;
        entity.UpdatedBy = SystemUser;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task QuarantineSourceAsync(
        Common.Domain.Entities.Source entity,
        string stagingKey,
        string? reason,
        CancellationToken cancellationToken)
    {
        var quarantineKey = SourceStorageKey.ToQuarantineKey(stagingKey);
        await objectStorage.CopyAsync(stagingKey, quarantineKey, cancellationToken);
        await objectStorage.DeleteAsync(stagingKey, cancellationToken);

        entity.StorageKey = quarantineKey;
        entity.Locator = quarantineKey;
        entity.UploadStatus = SourceUploadStatus.Quarantined;
        entity.UpdatedBy = SystemUser;
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogWarning(
            "Uploaded source {SourceId} was quarantined. Reason: {Reason}",
            entity.Id,
            reason ?? "Scanner returned unsafe verdict.");
    }

    private static bool ClientChecksumMatches(string? clientChecksum, string computedChecksum)
    {
        if (string.IsNullOrWhiteSpace(clientChecksum))
        {
            return true;
        }

        var normalized = clientChecksum.Trim().ToLowerInvariant();
        if (!normalized.StartsWith("sha256:", StringComparison.Ordinal))
        {
            normalized = $"sha256:{normalized}";
        }

        return FixedTimeEquals(normalized, computedChecksum);
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length &&
               CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private sealed record ObjectEvidence(long SizeBytes, string Sha256Hex, byte[] Prefix);
}
