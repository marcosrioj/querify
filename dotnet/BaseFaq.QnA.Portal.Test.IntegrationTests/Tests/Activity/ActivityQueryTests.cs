using BaseFaq.Models.QnA.Dtos.Activity;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.Activity.Queries.GetActivity;
using BaseFaq.QnA.Portal.Business.Activity.Queries.GetActivityList;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.Activity;

public class ActivityQueryTests
{
    [Fact]
    public async Task GetActivityList_FiltersByQuestionAndReturnsRecentEntries()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question =
            await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);
        var answer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            question.Id,
            accept: true);

        var listHandler =
            new ActivitiesGetActivityListQueryHandler(context.DbContext, context.SessionService);
        var result = await listHandler.Handle(new ActivitiesGetActivityListQuery
        {
            Request = new ActivityGetAllRequestDto
            {
                QuestionId = question.Id,
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "occurredAtUtc DESC"
            }
        }, CancellationToken.None);

        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, item => Assert.Equal(question.Id, item.QuestionId));
        Assert.All(result.Items, item => Assert.Equal(question.Title, item.QuestionTitle));
        Assert.Contains(result.Items, item => item.Kind == ActivityKind.AnswerActive);
        Assert.Contains(
            result.Items,
            item => item.AnswerId == answer.Id && item.AnswerHeadline == answer.Headline);
    }

    [Fact]
    public async Task GetActivity_ReturnsQuestionTitleAndAnswerHeadline()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question =
            await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);
        var answer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            question.Id);

        var activity = context.DbContext.Activities.Single(entity => entity.AnswerId == answer.Id);
        var handler = new ActivitiesGetActivityQueryHandler(context.DbContext, context.SessionService);
        var result = await handler.Handle(
            new ActivitiesGetActivityQuery { Id = activity.Id },
            CancellationToken.None);

        Assert.Equal(question.Id, result.QuestionId);
        Assert.Equal(question.Title, result.QuestionTitle);
        Assert.Equal(answer.Id, result.AnswerId);
        Assert.Equal(answer.Headline, result.AnswerHeadline);
    }
}
