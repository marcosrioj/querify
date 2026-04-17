using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Question.Commands.ApproveQuestion;
using BaseFaq.QnA.Portal.Business.Question.Commands.EscalateQuestion;
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
            visibility: VisibilityScope.Internal);

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

        Assert.Equal(QuestionStatus.Open, result.Status);
        Assert.Contains(result.Activity, activity => activity.Kind == ActivityKind.QuestionSubmitted);
        Assert.Contains(result.Activity, activity => activity.Kind == ActivityKind.QuestionApproved);
    }

    [Fact]
    public async Task RejectAndEscalateQuestion_RecordNotesAndVisibilityChanges()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var rejectedQuestion = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id,
            status: QuestionStatus.PendingReview,
            visibility: VisibilityScope.Internal);
        var escalatedQuestion = await TestDataFactory.SeedQuestionAsync(
            context.DbContext,
            context.SessionService.TenantId,
            space.Id,
            status: QuestionStatus.Open,
            visibility: VisibilityScope.Internal);

        await new QuestionsRejectQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new QuestionsRejectQuestionCommand
        {
            Id = rejectedQuestion.Id,
            Notes = "Missing customer context"
        }, CancellationToken.None);

        await new QuestionsEscalateQuestionCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new QuestionsEscalateQuestionCommand
        {
            Id = escalatedQuestion.Id,
            Notes = "Needs human support follow-up"
        }, CancellationToken.None);

        var rejectResult = await new QuestionsGetQuestionQueryHandler(
            context.DbContext,
            context.SessionService).Handle(new QuestionsGetQuestionQuery
        {
            Id = rejectedQuestion.Id
        }, CancellationToken.None);
        var escalateResult = await new QuestionsGetQuestionQueryHandler(
            context.DbContext,
            context.SessionService).Handle(new QuestionsGetQuestionQuery
        {
            Id = escalatedQuestion.Id
        }, CancellationToken.None);

        Assert.Equal(QuestionStatus.Draft, rejectResult.Status);
        Assert.Equal(VisibilityScope.Internal, rejectResult.Visibility);
        Assert.Contains(rejectResult.Activity,
            activity => activity.Kind == ActivityKind.QuestionRejected &&
                        activity.Notes == "Missing customer context");

        Assert.Equal(QuestionStatus.Escalated, escalateResult.Status);
        Assert.Contains(escalateResult.Activity,
            activity => activity.Kind == ActivityKind.QuestionEscalated &&
                        activity.Notes == "Needs human support follow-up");
    }
}
