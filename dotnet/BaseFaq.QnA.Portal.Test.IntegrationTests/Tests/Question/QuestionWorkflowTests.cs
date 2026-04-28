using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Question.Commands.ApproveQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.RejectQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.SubmitQuestion;
using BaseFaq.QnA.Portal.Business.Question.Queries.GetQuestion;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Question;

public class QuestionWorkflowTests
{
    [Fact]
    public async Task SubmitAndApproveQuestion_TransitionsThroughModerationWorkflow()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id,
            status: QuestionStatus.Draft,
            visibility: VisibilityScope.Authenticated);

        await new QuestionsSubmitQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new QuestionsSubmitQuestionCommand
        {
            Id = question.Id
        }, CancellationToken.None);

        await new QuestionsApproveQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new QuestionsApproveQuestionCommand
        {
            Id = question.Id
        }, CancellationToken.None);

        var result = await new QuestionsGetQuestionQueryHandler(
            context.DbContext,
            context.SessionService).Handle(new QuestionsGetQuestionQuery
        {
            Id = question.Id
        }, CancellationToken.None);

        Assert.Equal(QuestionStatus.Active, result.Status);
        Assert.Contains(result.Activity, activity => activity.Kind == ActivityKind.QuestionSubmitted);
        Assert.Contains(result.Activity, activity => activity.Kind == ActivityKind.QuestionApproved);
    }

    [Fact]
    public async Task RejectQuestion_RecordsNotesAndVisibilityChanges()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var rejectedQuestion = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id,
            status: QuestionStatus.Draft,
            visibility: VisibilityScope.Authenticated);
        await new QuestionsRejectQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new QuestionsRejectQuestionCommand
        {
            Id = rejectedQuestion.Id,
            Notes = "Missing customer context"
        }, CancellationToken.None);

        var rejectResult = await new QuestionsGetQuestionQueryHandler(
            context.DbContext,
            context.SessionService).Handle(new QuestionsGetQuestionQuery
        {
            Id = rejectedQuestion.Id
        }, CancellationToken.None);
        Assert.Equal(QuestionStatus.Draft, rejectResult.Status);
        Assert.Equal(VisibilityScope.Authenticated, rejectResult.Visibility);
        Assert.Contains(rejectResult.Activity,
            activity => activity.Kind == ActivityKind.QuestionRejected &&
                        activity.Notes == "Missing customer context");
    }
}
