using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Sources;
using Querify.QnA.Common.Domain.Options;
using Querify.QnA.Common.Domain.Entities;
using Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSource;
using Querify.QnA.Worker.Test.IntegrationTests.Helpers;
using Xunit;

namespace Querify.QnA.Worker.Test.IntegrationTests.Tests.Source;

public class VerifyUploadedSourceCommandHandlerTests
{
    [Fact]
    public async Task HappyPath_CopiesStagingToVerifiedDeletesStagingAndTransitionsToVerified()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var content = "%PDF-1.7 test"u8.ToArray();
        var source = await SeedUploadedSourceAsync(context, storage, content);
        source.UploadChecksum = BuildSha256(content);
        await context.DbContext.SaveChangesAsync();
        var handler = CreateHandler(context, storage);

        await handler.Handle(new VerifyUploadedSourceCommand
        {
            TenantId = context.SessionService.TenantId,
            SourceId = source.Id,
            StorageKey = source.StorageKey!
        }, CancellationToken.None);

        Assert.Equal(SourceUploadStatus.Verified, source.UploadStatus);
        Assert.Null(source.UploadChecksum);
        Assert.True(SourceStorageKey.IsVerifiedKey(source.StorageKey));
        Assert.Equal(source.StorageKey, source.Locator);
        Assert.NotNull(source.LastVerifiedAtUtc);
        Assert.Contains(storage.CopiedKeys, item => item.DestinationKey == source.StorageKey);
        Assert.Contains(storage.DeletedKeys, key => SourceStorageKey.IsStagingKey(key));
    }

    [Fact]
    public async Task ChecksumMismatch_TransitionsToFailed()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var source = await SeedUploadedSourceAsync(context, storage, "%PDF-1.7 test"u8.ToArray());
        source.UploadChecksum = "sha256:0000";
        await context.DbContext.SaveChangesAsync();
        var handler = CreateHandler(context, storage);

        await handler.Handle(new VerifyUploadedSourceCommand
        {
            TenantId = context.SessionService.TenantId,
            SourceId = source.Id,
            StorageKey = source.StorageKey!
        }, CancellationToken.None);

        Assert.Equal(SourceUploadStatus.Failed, source.UploadStatus);
        Assert.Null(source.UploadChecksum);
        Assert.Contains(source.StorageKey!, storage.DeletedKeys);
    }

    [Fact]
    public async Task MagicBytesMismatch_TransitionsToFailed()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var source = await SeedUploadedSourceAsync(context, storage, "not a pdf"u8.ToArray());
        var handler = CreateHandler(context, storage);

        await handler.Handle(new VerifyUploadedSourceCommand
        {
            TenantId = context.SessionService.TenantId,
            SourceId = source.Id,
            StorageKey = source.StorageKey!
        }, CancellationToken.None);

        Assert.Equal(SourceUploadStatus.Failed, source.UploadStatus);
        Assert.Null(source.UploadChecksum);
        Assert.Contains(source.StorageKey!, storage.DeletedKeys);
    }

    [Fact]
    public async Task MalwareDetected_TransitionsToQuarantined()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var source = await SeedUploadedSourceAsync(context, storage, "%PDF-1.7 test"u8.ToArray());
        var handler = CreateHandler(context, storage, scannerIsSafe: false);

        await handler.Handle(new VerifyUploadedSourceCommand
        {
            TenantId = context.SessionService.TenantId,
            SourceId = source.Id,
            StorageKey = source.StorageKey!
        }, CancellationToken.None);

        Assert.Equal(SourceUploadStatus.Quarantined, source.UploadStatus);
        Assert.Null(source.UploadChecksum);
        Assert.Contains("/quarantine/", source.StorageKey, StringComparison.Ordinal);
        Assert.Equal(source.StorageKey, source.Locator);
    }

    [Fact]
    public async Task StatusNotUploaded_IsIdempotent()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var source = await SeedUploadedSourceAsync(context, storage, "%PDF-1.7 test"u8.ToArray(),
            SourceUploadStatus.Pending);
        var handler = CreateHandler(context, storage);

        await handler.Handle(new VerifyUploadedSourceCommand
        {
            TenantId = context.SessionService.TenantId,
            SourceId = source.Id,
            StorageKey = source.StorageKey!
        }, CancellationToken.None);

        Assert.Equal(SourceUploadStatus.Pending, source.UploadStatus);
        Assert.Empty(storage.DeletedKeys);
        Assert.Empty(storage.CopiedKeys);
    }

    [Fact]
    public async Task StaleDuplicateAfterVerified_DoesNotOverwriteVerifiedStatus()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var content = "%PDF-1.7 test"u8.ToArray();
        var source = await SeedUploadedSourceAsync(context, storage, content);
        var stagingKey = source.StorageKey!;
        var verifiedKey = SourceStorageKey.ToVerifiedKey(stagingKey);
        var computedChecksum = BuildSha256(content);
        var verifiedAtUtc = DateTime.UtcNow;

        storage.Put(verifiedKey, content, "application/pdf");
        await context.DbContext.Sources
            .Where(item => item.TenantId == source.TenantId && item.Id == source.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.StorageKey, verifiedKey)
                .SetProperty(item => item.Locator, verifiedKey)
                .SetProperty(item => item.Checksum, computedChecksum)
                .SetProperty(item => item.UploadChecksum, (string?)null)
                .SetProperty(item => item.LastVerifiedAtUtc, verifiedAtUtc)
                .SetProperty(item => item.UploadStatus, SourceUploadStatus.Verified));
        await storage.DeleteAsync(stagingKey, CancellationToken.None);
        var handler = CreateHandler(context, storage);

        await handler.Handle(new VerifyUploadedSourceCommand
        {
            TenantId = context.SessionService.TenantId,
            SourceId = source.Id,
            StorageKey = stagingKey
        }, CancellationToken.None);

        await context.DbContext.Entry(source).ReloadAsync();
        Assert.Equal(SourceUploadStatus.Verified, source.UploadStatus);
        Assert.Equal(verifiedKey, source.StorageKey);
        Assert.Equal(verifiedKey, source.Locator);
        Assert.Equal(computedChecksum, source.Checksum);
    }

    private static VerifyUploadedSourceCommandHandler CreateHandler(
        TestContext context,
        FakeObjectStorage storage,
        bool scannerIsSafe = true)
    {
        return new VerifyUploadedSourceCommandHandler(
            context.DbContext,
            storage,
            new FakeThreatScanner(scannerIsSafe),
            Options.Create(new SourceUploadOptions()),
            NullLogger<VerifyUploadedSourceCommandHandler>.Instance);
    }

    private static async Task<Common.Domain.Entities.Source> SeedUploadedSourceAsync(
        TestContext context,
        FakeObjectStorage storage,
        byte[] content,
        SourceUploadStatus uploadStatus = SourceUploadStatus.Uploaded)
    {
        var sourceId = Guid.NewGuid();
        var storageKey = SourceStorageKey.BuildStagingKey(context.SessionService.TenantId, sourceId, "manual.pdf");
        var source = new Common.Domain.Entities.Source
        {
            Id = sourceId,
            TenantId = context.SessionService.TenantId,
            Kind = SourceKind.Pdf,
            Locator = storageKey,
            StorageKey = storageKey,
            Label = "Manual",
            Language = "en-US",
            MediaType = "application/pdf",
            SizeBytes = content.LongLength,
            Checksum = "sha256:pending",
            UploadStatus = uploadStatus,
            Visibility = VisibilityScope.Internal,
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        context.DbContext.Sources.Add(source);
        await context.DbContext.SaveChangesAsync();
        storage.Put(storageKey, content, "application/pdf");
        return source;
    }

    private static string BuildSha256(byte[] content)
    {
        return $"sha256:{Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant()}";
    }
}
