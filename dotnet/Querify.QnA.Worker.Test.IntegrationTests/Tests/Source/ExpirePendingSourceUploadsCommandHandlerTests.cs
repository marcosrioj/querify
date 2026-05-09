using Microsoft.Extensions.Options;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Sources;
using Querify.QnA.Common.Domain.Options;
using Querify.QnA.Worker.Business.Source.Commands.ExpirePendingSourceUploads;
using Querify.QnA.Worker.Test.IntegrationTests.Helpers;
using Xunit;

namespace Querify.QnA.Worker.Test.IntegrationTests.Tests.Source;

public class ExpirePendingSourceUploadsCommandHandlerTests
{
    [Fact]
    public async Task ExpiredPendingSource_DeletesStagingObjectAndTransitionsToExpired()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var source = await SeedPendingSourceAsync(context, storage, DateTime.UtcNow.AddHours(-25));
        var handler = CreateHandler(context, storage);

        var expiredAny = await handler.Handle(new ExpirePendingSourceUploadsCommand { NowUtc = DateTime.UtcNow },
            CancellationToken.None);

        Assert.True(expiredAny);
        Assert.Equal(SourceUploadStatus.Expired, source.UploadStatus);
        Assert.Contains(source.StorageKey!, storage.DeletedKeys);
    }

    [Fact]
    public async Task FreshPendingSource_IsIgnored()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var source = await SeedPendingSourceAsync(context, storage, DateTime.UtcNow.AddHours(-1));
        var handler = CreateHandler(context, storage);

        var expiredAny = await handler.Handle(new ExpirePendingSourceUploadsCommand { NowUtc = DateTime.UtcNow },
            CancellationToken.None);

        Assert.False(expiredAny);
        Assert.Equal(SourceUploadStatus.Pending, source.UploadStatus);
        Assert.Empty(storage.DeletedKeys);
    }

    private static ExpirePendingSourceUploadsCommandHandler CreateHandler(
        TestContext context,
        FakeObjectStorage storage)
    {
        return new ExpirePendingSourceUploadsCommandHandler(
            context.DbContext,
            storage,
            Options.Create(new SourceUploadOptions { PendingExpirationHours = 24 }));
    }

    private static async Task<Common.Domain.Entities.Source> SeedPendingSourceAsync(
        TestContext context,
        FakeObjectStorage storage,
        DateTime createdAtUtc)
    {
        var sourceId = Guid.NewGuid();
        var storageKey = SourceStorageKey.BuildStagingKey(context.SessionService.TenantId, sourceId, "manual.pdf");
        var source = new Common.Domain.Entities.Source
        {
            Id = sourceId,
            TenantId = context.SessionService.TenantId,
            Locator = storageKey,
            StorageKey = storageKey,
            Label = "Manual",
            Language = "en-US",
            MediaType = "application/pdf",
            SizeBytes = 12,
            Checksum = SourceChecksum.FromLocator(storageKey),
            UploadStatus = SourceUploadStatus.Pending,
            CreatedDate = createdAtUtc,
            CreatedBy = "test",
            UpdatedBy = "test"
        };

        context.DbContext.Sources.Add(source);
        await context.DbContext.SaveChangesAsync();
        storage.Put(storageKey, "%PDF-1.7 test"u8.ToArray(), "application/pdf");
        return source;
    }
}
