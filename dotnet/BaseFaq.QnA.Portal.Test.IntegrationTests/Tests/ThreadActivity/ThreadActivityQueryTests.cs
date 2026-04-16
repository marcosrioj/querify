using BaseFaq.Models.QnA.Dtos.ThreadActivity;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Portal.Business.ThreadActivity.Queries.GetThreadActivity;
using BaseFaq.QnA.Portal.Business.ThreadActivity.Queries.GetThreadActivityList;
using BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;
using Xunit;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Tests.ThreadActivity;

public class ThreadActivityQueryTests
{
    [Fact]
    public async Task GetThreadActivityList_FiltersByQuestionAndReturnsRecentEntries()
    {
        using var context = TestContext.Create();
        var space = await TestDataFactory.SeedQuestionSpaceAsync(context.DbContext, context.SessionService.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.SessionService.TenantId, space.Id);
        await TestDataFactory.SeedAnswerAsync(context.DbContext, context.SessionService.TenantId, question.Id, accept: true);

        var listHandler = new ThreadActivitiesGetThreadActivityListQueryHandler(context.DbContext, context.SessionService);
        var result = await listHandler.Handle(new ThreadActivitiesGetThreadActivityListQuery
        {
            Request = new ThreadActivityGetAllRequestDto
            {
                QuestionId = question.Id,
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "occurredAtUtc DESC"
            }
        }, CancellationToken.None);

        Assert.NotEmpty(result.Items);
        Assert.All(result.Items, item => Assert.Equal(question.Id, item.QuestionId));
        Assert.Contains(result.Items, item => item.Kind == ActivityKind.AnswerAccepted);
    }
}
