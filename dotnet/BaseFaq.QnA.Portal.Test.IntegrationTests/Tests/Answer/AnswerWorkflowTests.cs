using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Answer.Commands.PublishAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.RejectAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.RetireAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Commands.ValidateAnswer;
using BaseFaq.QnA.Portal.Business.Answer.Queries.GetAnswer;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Answer;

public class AnswerWorkflowTests
{
    [Fact]
    public async Task PublishAndValidateAnswer_TransitionsLifecycleAndAppendsActivity()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);
        var answer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            question.Id,
            status: AnswerStatus.Draft,
            visibility: VisibilityScope.Internal,
            accept: false);

        await new AnswersPublishAnswerCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new AnswersPublishAnswerCommand
        {
            Id = answer.Id
        }, CancellationToken.None);

        await new AnswersValidateAnswerCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new AnswersValidateAnswerCommand
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

        Assert.Equal(AnswerStatus.Validated, result.Status);
        Assert.NotNull(result.PublishedAtUtc);
        Assert.NotNull(result.ValidatedAtUtc);
        Assert.Contains(ActivityKind.AnswerPublished, activityKinds);
        Assert.Contains(ActivityKind.AnswerValidated, activityKinds);
    }

    [Fact]
    public async Task RejectAndRetireAnswer_EnforcesInternalVisibilityAndRetirementTrail()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);
        var rejectedAnswer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            question.Id,
            status: AnswerStatus.Published,
            visibility: VisibilityScope.PublicIndexed,
            accept: false);
        var retiredAnswer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            question.Id,
            status: AnswerStatus.Published,
            visibility: VisibilityScope.PublicIndexed,
            accept: false,
            rank: 2);

        await new AnswersRejectAnswerCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new AnswersRejectAnswerCommand
        {
            Id = rejectedAnswer.Id
        }, CancellationToken.None);

        await new AnswersRetireAnswerCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor).Handle(new AnswersRetireAnswerCommand
        {
            Id = retiredAnswer.Id
        }, CancellationToken.None);

        var rejectedResult = await new AnswersGetAnswerQueryHandler(
            context.DbContext,
            context.SessionService).Handle(new AnswersGetAnswerQuery
        {
            Id = rejectedAnswer.Id
        }, CancellationToken.None);
        var retiredResult = await new AnswersGetAnswerQueryHandler(
            context.DbContext,
            context.SessionService).Handle(new AnswersGetAnswerQuery
        {
            Id = retiredAnswer.Id
        }, CancellationToken.None);
        var rejectedActivityKinds = context.DbContext.Activities
            .Where(activity => activity.AnswerId == rejectedAnswer.Id)
            .Select(activity => activity.Kind)
            .ToList();
        var retiredActivityKinds = context.DbContext.Activities
            .Where(activity => activity.AnswerId == retiredAnswer.Id)
            .Select(activity => activity.Kind)
            .ToList();

        Assert.Equal(AnswerStatus.Rejected, rejectedResult.Status);
        Assert.Equal(VisibilityScope.Internal, rejectedResult.Visibility);
        Assert.Contains(ActivityKind.AnswerRejected, rejectedActivityKinds);

        Assert.Equal(AnswerStatus.Archived, retiredResult.Status);
        Assert.Equal(VisibilityScope.Internal, retiredResult.Visibility);
        Assert.NotNull(retiredResult.RetiredAtUtc);
        Assert.Contains(ActivityKind.AnswerRetired, retiredActivityKinds);
    }
}
