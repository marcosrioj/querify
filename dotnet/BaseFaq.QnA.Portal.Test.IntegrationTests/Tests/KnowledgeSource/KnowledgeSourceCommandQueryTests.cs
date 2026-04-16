using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Commands.CreateKnowledgeSource;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Queries.GetKnowledgeSource;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.KnowledgeSource;

public class KnowledgeSourceCommandQueryTests
{
    [Fact]
    public async Task CreateKnowledgeSource_PersistsEntityAndReturnsDto()
    {
        using var context = TestContext.Create();

        var createHandler =
            new KnowledgeSourcesCreateKnowledgeSourceCommandHandler(context.DbContext, context.SessionService);
        var id = await createHandler.Handle(new KnowledgeSourcesCreateKnowledgeSourceCommand
        {
            Request = new KnowledgeSourceCreateRequestDto
            {
                Kind = SourceKind.Article,
                Locator = "https://docs.example.test/qna/reset-password",
                Label = "Reset password guide",
                Scope = "Portal",
                SystemName = "docs",
                ExternalId = "DOC-42",
                Language = "en-US",
                MediaType = "text/html",
                MetadataJson = "{\"category\":\"support\"}",
                Visibility = VisibilityScope.Internal,
                AllowsPublicCitation = false,
                AllowsPublicExcerpt = false,
                IsAuthoritative = true,
                MarkVerified = true
            }
        }, CancellationToken.None);

        var getHandler = new KnowledgeSourcesGetKnowledgeSourceQueryHandler(context.DbContext, context.SessionService);
        var result = await getHandler.Handle(new KnowledgeSourcesGetKnowledgeSourceQuery { Id = id },
            CancellationToken.None);

        Assert.Equal("https://docs.example.test/qna/reset-password", result.Locator);
        Assert.Equal("Reset password guide", result.Label);
        Assert.Equal(VisibilityScope.Internal, result.Visibility);
        Assert.False(result.AllowsPublicCitation);
    }
}