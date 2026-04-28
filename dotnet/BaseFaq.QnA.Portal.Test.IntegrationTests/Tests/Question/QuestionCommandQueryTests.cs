using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Question.Commands.UpdateQuestion;
using BaseFaq.QnA.Portal.Business.Question.Queries.GetQuestion;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Question;

public class QuestionCommandQueryTests
{
    [Fact]
    public async Task UpdateQuestion_AcceptsAnswerAndWritesActivity()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id,
            visibility: VisibilityScope.Public);
        var acceptedAnswer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            question.Id,
            visibility: VisibilityScope.Public,
            accept: false);
        var updateHandler = new QuestionsUpdateQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        await updateHandler.Handle(new QuestionsUpdateQuestionCommand
        {
            Id = question.Id,
            Request = new QuestionUpdateRequestDto
            {
                Title = question.Title,
                Summary = question.Summary,
                ContextNote = question.ContextNote,
                Status = question.Status,
                Visibility = question.Visibility,
                OriginChannel = question.OriginChannel,
                Sort = question.Sort,
                AcceptedAnswerId = acceptedAnswer.Id
            }
        }, CancellationToken.None);

        var getHandler = new QuestionsGetQuestionQueryHandler(context.DbContext, context.SessionService);
        var result =
            await getHandler.Handle(new QuestionsGetQuestionQuery { Id = question.Id }, CancellationToken.None);

        Assert.Equal(acceptedAnswer.Id, result.AcceptedAnswerId);
        Assert.Equal(QuestionStatus.Active, result.Status);
        Assert.NotNull(result.AcceptedAnswer);
        Assert.True(result.AcceptedAnswer!.IsAccepted);
        Assert.Contains(result.Activity, activity => activity.Kind == ActivityKind.AnswerAccepted);
    }
}
