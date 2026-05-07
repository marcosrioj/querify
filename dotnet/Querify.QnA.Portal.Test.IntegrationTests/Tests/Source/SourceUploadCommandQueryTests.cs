using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Storage.Options;
using Querify.Models.QnA.Dtos.Source;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.BusinessRules.Sources;
using Querify.QnA.Common.Domain.Options;
using Querify.QnA.Portal.Business.Source.Commands.CompleteUpload;
using Querify.QnA.Portal.Business.Source.Commands.CreateUploadIntent;
using Querify.QnA.Portal.Business.Source.Queries.GetDownloadUrl;
using Querify.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace Querify.QnA.Portal.Test.IntegrationTests.Tests.Source;

public class SourceUploadCommandQueryTests
{
    private static SourceUploadOptions UploadOptions => new();

    [Fact]
    public async Task CreateUploadIntent_ValidPdf_CreatesPendingSourceAndReturnsPresignedUrl()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var handler = new SourcesCreateUploadIntentCommandHandler(
            context.DbContext,
            context.SessionService,
            storage,
            Options.Create(UploadOptions));

        var result = await handler.Handle(new SourcesCreateUploadIntentCommand
        {
            Dto = ValidIntentRequest()
        }, CancellationToken.None);

        var source = await context.DbContext.Sources.SingleAsync(source => source.Id == result.SourceId);
        Assert.Equal(SourceUploadStatus.Pending, source.UploadStatus);
        Assert.Equal(source.StorageKey, source.Locator);
        Assert.True(SourceStorageKey.IsStagingKey(source.StorageKey));
        Assert.Equal("application/pdf", source.MediaType);
        Assert.Equal("https://storage.example.test/", $"{result.UploadUrl[..29]}");
        Assert.Equal("application/pdf", result.RequiredHeaders["Content-Type"]);
    }

    [Theory]
    [InlineData("application/x-msdownload", "manual.exe", HttpStatusCode.UnprocessableEntity)]
    [InlineData("application/pdf", "manual.exe", HttpStatusCode.UnprocessableEntity)]
    public async Task CreateUploadIntent_InvalidContentTypeOrExtension_Returns422(
        string contentType,
        string fileName,
        HttpStatusCode statusCode)
    {
        using var context = TestContext.Create();
        var handler = CreateIntentHandler(context, new FakeObjectStorage());
        var request = ValidIntentRequest();
        request.ContentType = contentType;
        request.FileName = fileName;

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new SourcesCreateUploadIntentCommand { Dto = request },
            CancellationToken.None));

        Assert.Equal((int)statusCode, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateUploadIntent_OversizeBytes_Returns422()
    {
        using var context = TestContext.Create();
        var handler = CreateIntentHandler(context, new FakeObjectStorage());
        var request = ValidIntentRequest();
        request.SizeBytes = SourceUploadOptions.DefaultMaxUploadBytes + 1;

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new SourcesCreateUploadIntentCommand { Dto = request },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateUploadIntent_PublicVisibility_Returns422()
    {
        using var context = TestContext.Create();
        var handler = CreateIntentHandler(context, new FakeObjectStorage());
        var request = ValidIntentRequest();
        request.Visibility = VisibilityScope.Public;

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new SourcesCreateUploadIntentCommand { Dto = request },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
    }

    [Fact]
    public async Task CompleteUpload_NoBlobInStorage_Returns422()
    {
        using var context = TestContext.Create();
        var source = await CreatePendingUploadAsync(context, new FakeObjectStorage());
        var handler = CreateCompleteHandler(context, new FakeObjectStorage());

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new SourcesCompleteUploadCommand { SourceId = source.Id },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
    }

    [Fact]
    public async Task CompleteUpload_SizeMismatch_DeletesStagingObjectAndReturns422()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var source = await CreatePendingUploadAsync(context, storage, expectedSizeBytes: 12);
        storage.Put(source.StorageKey!, "tiny"u8.ToArray(), "application/pdf");
        var handler = CreateCompleteHandler(context, storage);

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new SourcesCompleteUploadCommand { SourceId = source.Id },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
        Assert.Contains(source.StorageKey!, storage.DeletedKeys);
        Assert.Equal(SourceUploadStatus.Failed, source.UploadStatus);
    }

    [Fact]
    public async Task CompleteUpload_ContentTypeMismatch_DeletesStagingObjectAndReturns422()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var source = await CreatePendingUploadAsync(context, storage, expectedSizeBytes: 4);
        storage.Put(source.StorageKey!, "test"u8.ToArray(), "text/plain");
        var handler = CreateCompleteHandler(context, storage);

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new SourcesCompleteUploadCommand { SourceId = source.Id },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
        Assert.Contains(source.StorageKey!, storage.DeletedKeys);
        Assert.Equal(SourceUploadStatus.Failed, source.UploadStatus);
    }

    [Fact]
    public async Task CompleteUpload_StatusNotPending_Returns409()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var source = await TestDataFactory.SeedVerifiedUploadedSourceAsync(context.DbContext,
            context.SessionService.TenantId);
        var handler = CreateCompleteHandler(context, storage);

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new SourcesCompleteUploadCommand { SourceId = source.Id },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.Conflict, exception.ErrorCode);
    }

    [Fact]
    public async Task CompleteUpload_HappyPath_TransitionsToUploadedAndStoresUploadChecksum()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var content = "%PDF-1.7 test"u8.ToArray();
        var source = await CreatePendingUploadAsync(context, storage, expectedSizeBytes: content.LongLength);
        storage.Put(source.StorageKey!, content, "application/pdf");
        var handler = CreateCompleteHandler(context, storage);

        var result = await handler.Handle(new SourcesCompleteUploadCommand
        {
            SourceId = source.Id,
            ClientChecksum = "sha256:client"
        }, CancellationToken.None);

        Assert.Equal(source.Id, result);
        Assert.Equal(SourceUploadStatus.Uploaded, source.UploadStatus);
        Assert.Equal("sha256:client", source.UploadChecksum);
    }

    [Fact]
    public async Task GetDownloadUrl_UrlSource_Returns422()
    {
        using var context = TestContext.Create();
        var source = await TestDataFactory.SeedSourceAsync(context.DbContext, context.SessionService.TenantId);
        var handler = CreateDownloadHandler(context, new FakeObjectStorage());

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new SourcesGetDownloadUrlQuery { Id = source.Id },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
    }

    [Fact]
    public async Task GetDownloadUrl_UploadedSource_Returns422()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var source = await CreatePendingUploadAsync(context, storage);
        source.UploadStatus = SourceUploadStatus.Uploaded;
        await context.DbContext.SaveChangesAsync();
        var handler = CreateDownloadHandler(context, storage);

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new SourcesGetDownloadUrlQuery { Id = source.Id },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
    }

    [Fact]
    public async Task GetDownloadUrl_VerifiedSource_ReturnsPresignedGet()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var source = await TestDataFactory.SeedVerifiedUploadedSourceAsync(context.DbContext,
            context.SessionService.TenantId);
        var handler = CreateDownloadHandler(context, storage);

        var result = await handler.Handle(new SourcesGetDownloadUrlQuery { Id = source.Id },
            CancellationToken.None);

        Assert.Contains("/download/", result.Url, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetDownloadUrl_QuarantinedSource_Returns422()
    {
        using var context = TestContext.Create();
        var storage = new FakeObjectStorage();
        var source = await CreatePendingUploadAsync(context, storage);
        source.StorageKey = SourceStorageKey.ToQuarantineKey(source.StorageKey!);
        source.Locator = source.StorageKey;
        source.UploadStatus = SourceUploadStatus.Quarantined;
        await context.DbContext.SaveChangesAsync();
        var handler = CreateDownloadHandler(context, storage);

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new SourcesGetDownloadUrlQuery { Id = source.Id },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
    }

    [Fact]
    public async Task CrossTenant_SourceVisibility_NotLeaked()
    {
        using var context = TestContext.Create();
        var otherTenantId = Guid.NewGuid();
        context.DbContext.TenantFiltersEnabled = false;
        var source = await TestDataFactory.SeedVerifiedUploadedSourceAsync(context.DbContext, otherTenantId);
        context.DbContext.TenantFiltersEnabled = true;
        var handler = CreateDownloadHandler(context, new FakeObjectStorage());

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new SourcesGetDownloadUrlQuery { Id = source.Id },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.NotFound, exception.ErrorCode);
    }

    private static SourcesCreateUploadIntentCommandHandler CreateIntentHandler(
        TestContext context,
        FakeObjectStorage storage)
    {
        return new SourcesCreateUploadIntentCommandHandler(
            context.DbContext,
            context.SessionService,
            storage,
            Options.Create(UploadOptions));
    }

    private static SourcesCompleteUploadCommandHandler CreateCompleteHandler(
        TestContext context,
        FakeObjectStorage storage)
    {
        return new SourcesCompleteUploadCommandHandler(
            context.DbContext,
            context.SessionService,
            storage,
            Options.Create(UploadOptions));
    }

    private static SourcesGetDownloadUrlQueryHandler CreateDownloadHandler(
        TestContext context,
        FakeObjectStorage storage)
    {
        return new SourcesGetDownloadUrlQueryHandler(
            context.DbContext,
            context.SessionService,
            storage,
            Options.Create(new ObjectStorageOptions()));
    }

    private static SourceUploadIntentRequestDto ValidIntentRequest()
    {
        return new SourceUploadIntentRequestDto
        {
            FileName = "manual.pdf",
            ContentType = "application/pdf",
            SizeBytes = 12,
            Kind = SourceKind.Pdf,
            Language = "en-US",
            Visibility = VisibilityScope.Internal,
            Label = "Manual",
            ContextNote = "Test upload"
        };
    }

    private static async Task<Common.Domain.Entities.Source> CreatePendingUploadAsync(
        TestContext context,
        FakeObjectStorage storage,
        long expectedSizeBytes = 12)
    {
        var handler = CreateIntentHandler(context, storage);
        var request = ValidIntentRequest();
        request.SizeBytes = expectedSizeBytes;
        var intent = await handler.Handle(new SourcesCreateUploadIntentCommand { Dto = request },
            CancellationToken.None);
        return await context.DbContext.Sources.SingleAsync(source => source.Id == intent.SourceId);
    }
}
