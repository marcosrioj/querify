using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Answer.Commands.CreateAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Queries.GetAnswer;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Answer;

public class AnswerCommandQueryTests
{
    [Fact]
    public async Task CreateAnswer_PersistsEntityAndReturnsDto()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question =
            await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);

        var createHandler = new AnswersCreateAnswerCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var id = await createHandler.Handle(new AnswersCreateAnswerCommand
        {
            Request = new AnswerCreateRequestDto
            {
                QuestionId = question.Id,
                Headline = "Reset password from portal",
                Body = "Open sign-in and request a password reset link.",
                Kind = AnswerKind.Official,
                Status = AnswerStatus.Published,
                Visibility = VisibilityScope.PublicIndexed,
                Language = "en-US",
                ContextKey = "portal",
                ConfidenceScore = 91,
                Rank = 2
            }
        }, CancellationToken.None);

        var getHandler = new AnswersGetAnswerQueryHandler(context.DbContext, context.SessionService);
        var result = await getHandler.Handle(new AnswersGetAnswerQuery { Id = id }, CancellationToken.None);

        Assert.Equal(question.Id, result.QuestionId);
        Assert.Equal("Reset password from portal", result.Headline);
        Assert.Equal(AnswerStatus.Published, result.Status);
        Assert.Equal(VisibilityScope.PublicIndexed, result.Visibility);
    }
}