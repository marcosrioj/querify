using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.EntityFramework.Core.AutoHistory;
using BaseFaq.Models.QnA.Dtos.Source;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Source.Commands.CreateSource;
using BaseFaq.QnA.Portal.Business.Source.Commands.UpdateSource;
using BaseFaq.QnA.Portal.Business.Source.Queries.GetSource;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Source;

public class SourceCommandQueryTests
{
    [Fact]
    public async Task CreateSource_PersistsEntityAndReturnsDto()
    {
        using var context = TestContext.Create();

        var createHandler =
            new SourcesCreateSourceCommandHandler(context.DbContext, context.SessionService);
        var id = await createHandler.Handle(new SourcesCreateSourceCommand
        {
            Request = new SourceCreateRequestDto
            {
                Kind = SourceKind.Article,
                Locator = "https://docs.example.test/qna/reset-password",
                Label = "Reset password guide",
                ContextNote = "Portal",
                ExternalId = "DOC-42",
                Language = "en-US",
                MediaType = "text/html",
                MetadataJson = "{\"category\":\"support\"}",
                Visibility = VisibilityScope.Authenticated,
                MarkVerified = true
            }
        }, CancellationToken.None);

        var getHandler = new SourcesGetSourceQueryHandler(context.DbContext, context.SessionService);
        var result = await getHandler.Handle(new SourcesGetSourceQuery { Id = id },
            CancellationToken.None);

        Assert.Equal("https://docs.example.test/qna/reset-password", result.Locator);
        Assert.Equal("Reset password guide", result.Label);
        Assert.Equal(VisibilityScope.Authenticated, result.Visibility);
        Assert.StartsWith("sha256:", result.Checksum);
        Assert.Equal(71, result.Checksum.Length);

        var history = await context.DbContext.Set<AutoHistory>().SingleAsync();
        Assert.Equal(id.ToString(), history.KeyId);
        Assert.Equal("Sources", history.TableName);
        Assert.Equal(EntityState.Added, history.Kind);
        Assert.Null(history.ChangedFrom);
        Assert.Contains("https://docs.example.test/qna/reset-password", history.ChangedTo);
    }

    [Fact]
    public async Task UpdateSource_RecomputesChecksumFromLocator()
    {
        using var context = TestContext.Create();
        var source = await TestDataFactory.SeedSourceAsync(context.DbContext, context.SessionService.TenantId);

        var updateHandler =
            new SourcesUpdateSourceCommandHandler(context.DbContext, context.SessionService);
        await updateHandler.Handle(new SourcesUpdateSourceCommand
        {
            Id = source.Id,
            Request = new SourceUpdateRequestDto
            {
                Kind = source.Kind,
                Locator = "https://docs.example.test/qna/updated-source",
                Label = source.Label,
                ContextNote = source.ContextNote,
                ExternalId = source.ExternalId,
                Language = source.Language,
                MediaType = source.MediaType,
                MetadataJson = source.MetadataJson,
                Visibility = source.Visibility,
                MarkVerified = false
            }
        }, CancellationToken.None);

        Assert.Equal("https://docs.example.test/qna/updated-source", source.Locator);
        Assert.NotEqual("sha256:test-source", source.Checksum);
        Assert.StartsWith("sha256:", source.Checksum);
        Assert.Equal(71, source.Checksum.Length);
    }

    [Fact]
    public async Task CreateSource_ReturnsApiErrorWhenPublicSourceIsUnverified()
    {
        using var context = TestContext.Create();

        var createHandler =
            new SourcesCreateSourceCommandHandler(context.DbContext, context.SessionService);

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => createHandler.Handle(
            new SourcesCreateSourceCommand
            {
                Request = new SourceCreateRequestDto
                {
                    Kind = SourceKind.Article,
                    Locator = "https://docs.example.test/qna/unverified",
                    Label = "Unverified source",
                    ContextNote = null,
                    ExternalId = null,
                    Language = "en-US",
                    MediaType = "text/html",
                    MetadataJson = null,
                    Visibility = VisibilityScope.Public,
                    MarkVerified = false
                }
            },
            CancellationToken.None));

        Assert.Equal((int)HttpStatusCode.UnprocessableEntity, exception.ErrorCode);
    }
}
