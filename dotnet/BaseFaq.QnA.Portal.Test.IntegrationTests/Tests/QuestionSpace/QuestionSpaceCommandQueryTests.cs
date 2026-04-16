using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Commands;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Queries;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.QuestionSpace;

public class QuestionSpaceCommandQueryTests
{
    [Fact]
    public async Task CreateQuestionSpace_PersistsGovernanceAndExposure()
    {
        using var context = TestContext.Create();
        var createHandler = new QuestionSpacesCreateQuestionSpaceCommandHandler(context.DbContext, context.SessionService);

        var id = await createHandler.Handle(new QuestionSpacesCreateQuestionSpaceCommand
        {
            Request = new QuestionSpaceCreateRequestDto
            {
                Name = "Portal Support",
                Key = "portal-support",
                DefaultLanguage = "en-US",
                Summary = "Support questions for portal users.",
                Kind = SpaceKind.CuratedKnowledge,
                Visibility = VisibilityScope.PublicIndexed,
                ModerationPolicy = ModerationPolicy.PostModeration,
                SearchMarkupMode = SearchMarkupMode.Hybrid,
                ProductScope = "Portal",
                JourneyScope = "Support",
                AcceptsQuestions = true,
                AcceptsAnswers = true,
                RequiresQuestionReview = false,
                RequiresAnswerReview = true,
                MarkValidated = true
            }
        }, CancellationToken.None);

        var getHandler = new QuestionSpacesGetQuestionSpaceQueryHandler(context.DbContext, context.SessionService);
        var result = await getHandler.Handle(new QuestionSpacesGetQuestionSpaceQuery { Id = id }, CancellationToken.None);

        Assert.Equal("Portal Support", result.Name);
        Assert.Equal("portal-support", result.Key);
        Assert.Equal(VisibilityScope.PublicIndexed, result.Visibility);
        Assert.Equal(SearchMarkupMode.Hybrid, result.SearchMarkupMode);
        Assert.Equal(ModerationPolicy.PostModeration, result.ModerationPolicy);
        Assert.True(result.AcceptsQuestions);
        Assert.NotNull(result.LastValidatedAtUtc);
    }
}
