using BaseFaq.Models.QnA.Dtos.Source;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Source.Commands.CreateSource;
using BaseFaq.QnA.Portal.Business.Source.Queries.GetSource;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
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
                Checksum = "sha256:doc-42",
                MetadataJson = "{\"category\":\"support\"}",
                Visibility = VisibilityScope.Internal,
                AllowsPublicCitation = false,
                AllowsPublicExcerpt = false,
                IsAuthoritative = true,
                MarkVerified = true
            }
        }, CancellationToken.None);

        var getHandler = new SourcesGetSourceQueryHandler(context.DbContext, context.SessionService);
        var result = await getHandler.Handle(new SourcesGetSourceQuery { Id = id },
            CancellationToken.None);

        Assert.Equal("https://docs.example.test/qna/reset-password", result.Locator);
        Assert.Equal("Reset password guide", result.Label);
        Assert.Equal(VisibilityScope.Internal, result.Visibility);
        Assert.False(result.AllowsPublicCitation);
    }
}
