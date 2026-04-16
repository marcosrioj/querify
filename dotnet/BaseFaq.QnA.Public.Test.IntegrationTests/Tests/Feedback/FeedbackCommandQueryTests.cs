using System.Net;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.QnA.Public.Business.Feedback.Commands;
using BaseFaq.QnA.Public.Business.Question.Queries;
using BaseFaq.QnA.Public.Test.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Tests.Feedback;

public class FeedbackCommandQueryTests
{
    [Fact]
    public async Task CreateFeedback_UpdatesQuestionFeedbackScoreFromLatestSignal()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.44");
        httpContext.Request.Headers.UserAgent = "QnAPublicFeedback/1.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var space = await TestDataFactory.SeedQuestionSpaceAsync(context.DbContext, context.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.TenantId, space.Id);
        var feedbackHandler = new FeedbacksCreateFeedbackCommandHandler(context.DbContext, context.HttpContextAccessor);

        await feedbackHandler.Handle(new FeedbacksCreateFeedbackCommand
        {
            Request = new QuestionFeedbackCreateRequestDto
            {
                QuestionId = question.Id,
                Like = true
            }
        }, CancellationToken.None);

        await feedbackHandler.Handle(new FeedbacksCreateFeedbackCommand
        {
            Request = new QuestionFeedbackCreateRequestDto
            {
                QuestionId = question.Id,
                Like = false,
                Reason = "Not relevant"
            }
        }, CancellationToken.None);

        var questionHandler = new QuestionsGetQuestionQueryHandler(
            context.DbContext,
            new TestSessionService(context.TenantId, context.UserId));
        var result = await questionHandler.Handle(new QuestionsGetQuestionQuery
        {
            Id = question.Id,
            Request = new QuestionGetRequestDto()
        }, CancellationToken.None);

        Assert.Equal(-1, result.FeedbackScore);
    }
}
