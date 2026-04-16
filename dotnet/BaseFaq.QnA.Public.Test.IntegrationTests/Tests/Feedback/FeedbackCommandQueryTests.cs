using System.Net;
using System.Security.Claims;
using BaseFaq.Common.Infrastructure.Core.Services;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.QnA.Common.Persistence.QnADb.Identity;
using BaseFaq.QnA.Common.Persistence.QnADb.Projections;
using BaseFaq.QnA.Public.Business.Feedback.Commands.CreateFeedback;
using BaseFaq.QnA.Public.Business.Question.Queries.GetQuestion;
using BaseFaq.QnA.Public.Test.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Tests.Feedback;

public class FeedbackCommandQueryTests
{
    [Fact]
    public async Task CreateFeedback_UsesStableUserPrintForAuthenticatedRequests()
    {
        var httpContext = CreateHttpContext(
            "192.0.2.44",
            "QnAPublicFeedback/1.0",
            externalUserId: "external-auth-user");

        using var context = TestContext.Create(httpContext: httpContext);
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.TenantId, space.Id);
        var feedbackHandler = CreateFeedbackHandler(context);

        var feedbackId = await feedbackHandler.Handle(new FeedbacksCreateFeedbackCommand
        {
            Request = new QuestionFeedbackCreateRequestDto
            {
                QuestionId = question.Id,
                Like = true
            }
        }, CancellationToken.None);

        var activity = await context.DbContext.Activities.FindAsync(feedbackId);
        var result = await CreateQuestionHandler(context).Handle(new QuestionsGetQuestionQuery
        {
            Id = question.Id,
            Request = new QuestionGetRequestDto()
        }, CancellationToken.None);

        Assert.NotNull(activity);
        Assert.Equal(context.UserId.ToString("D"), activity!.UserPrint);
        Assert.Equal("192.0.2.44", activity.Ip);
        Assert.Equal("QnAPublicFeedback/1.0", activity.UserAgent);
        Assert.Equal(context.UserId.ToString("D"), ActivitySignals.ParseFeedback(activity.MetadataJson)?.UserPrint);
        Assert.Equal(1, result.FeedbackScore);
    }

    [Fact]
    public async Task CreateFeedback_UsesFingerprintForAnonymousRequests()
    {
        var httpContext = CreateHttpContext("192.0.2.47", "QnAPublicFeedback/2.0");

        using var context = TestContext.Create(httpContext: httpContext);
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.TenantId, space.Id);
        var expectedIdentity = ActivityUserPrint.ResolveCurrent(
            context.HttpContextAccessor.HttpContext!,
            new ClaimService(context.HttpContextAccessor),
            context.SessionService);

        var feedbackId = await CreateFeedbackHandler(context).Handle(new FeedbacksCreateFeedbackCommand
        {
            Request = new QuestionFeedbackCreateRequestDto
            {
                QuestionId = question.Id,
                Like = false,
                Reason = "Not relevant"
            }
        }, CancellationToken.None);

        var activity = await context.DbContext.Activities.FindAsync(feedbackId);

        Assert.NotNull(activity);
        Assert.Equal(expectedIdentity.UserPrint, activity!.UserPrint);
        Assert.Equal(expectedIdentity.Ip, activity.Ip);
        Assert.Equal(expectedIdentity.UserAgent, activity.UserAgent);
        Assert.Equal(expectedIdentity.UserPrint, ActivitySignals.ParseFeedback(activity.MetadataJson)?.UserPrint);
    }

    private static FeedbacksCreateFeedbackCommandHandler CreateFeedbackHandler(TestContext context)
    {
        return new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.SessionService,
            new ClaimService(context.HttpContextAccessor),
            context.HttpContextAccessor);
    }

    private static QuestionsGetQuestionQueryHandler CreateQuestionHandler(TestContext context)
    {
        return new QuestionsGetQuestionQueryHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.HttpContextAccessor);
    }

    private static DefaultHttpContext CreateHttpContext(
        string ipAddress,
        string userAgent,
        string? externalUserId = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);
        httpContext.Request.Headers.UserAgent = userAgent;

        if (!string.IsNullOrWhiteSpace(externalUserId))
        {
            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    [new Claim("sub", externalUserId)],
                    authenticationType: "TestAuth"));
        }

        return httpContext;
    }
}
