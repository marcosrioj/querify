using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Answer.Commands.ActivateAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.RetireAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Queries.GetAnswer;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Answer;

public class AnswerWorkflowTests
{
    [Fact]
    public async Task ActivateAnswer_TransitionsLifecycleAndAppendsActivity()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);
        var answer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            question.Id,
            status: AnswerStatus.Draft,
            visibility: VisibilityScope.Authenticated,
            accept: false);

        await new AnswersActivateAnswerCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new AnswersActivateAnswerCommand
        {
            Id = answer.Id
        }, CancellationToken.None);

        var result = await new AnswersGetAnswerQueryHandler(
            context.DbContext,
            context.SessionService).Handle(new AnswersGetAnswerQuery
        {
            Id = answer.Id
        }, CancellationToken.None);
        var activityKinds = context.DbContext.Activities
            .Where(activity => activity.AnswerId == answer.Id)
            .Select(activity => activity.Kind)
            .ToList();

        Assert.Equal(AnswerStatus.Active, result.Status);
        Assert.NotNull(result.ActivatedAtUtc);
        Assert.Contains(ActivityKind.AnswerActivated, activityKinds);
    }

    [Fact]
    public async Task RetireAnswer_EnforcesInternalVisibilityAndRetirementTrail()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);
        var retiredAnswer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            question.Id,
            status: AnswerStatus.Active,
            visibility: VisibilityScope.Public,
            accept: false,
            rank: 2);

        await new AnswersRetireAnswerCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new AnswersRetireAnswerCommand
        {
            Id = retiredAnswer.Id
        }, CancellationToken.None);

        var retiredResult = await new AnswersGetAnswerQueryHandler(
            context.DbContext,
            context.SessionService).Handle(new AnswersGetAnswerQuery
        {
            Id = retiredAnswer.Id
        }, CancellationToken.None);
        var retiredActivityKinds = context.DbContext.Activities
            .Where(activity => activity.AnswerId == retiredAnswer.Id)
            .Select(activity => activity.Kind)
            .ToList();

        Assert.Equal(AnswerStatus.Archived, retiredResult.Status);
        Assert.Equal(VisibilityScope.Authenticated, retiredResult.Visibility);
        Assert.NotNull(retiredResult.RetiredAtUtc);
        Assert.Contains(ActivityKind.AnswerRetired, retiredActivityKinds);
    }
}
