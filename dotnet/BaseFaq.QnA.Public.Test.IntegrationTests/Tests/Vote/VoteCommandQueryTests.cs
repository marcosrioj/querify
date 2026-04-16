using System.Net;
using System.Security.Claims;
using BaseFaq.Common.Infrastructure.Core.Services;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.QnA.Common.Helper.Activities;
using BaseFaq.QnA.Public.Business.Question.Queries.GetQuestion;
using BaseFaq.QnA.Public.Business.Vote.Commands.CreateVote;
using BaseFaq.QnA.Public.Test.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Tests.Vote;

public class VoteCommandQueryTests
{
    [Fact]
    public async Task CreateVote_UsesStableUserPrintForAuthenticatedRequests()
    {
        var httpContext = CreateHttpContext(
            "192.0.2.45",
            "QnAPublicVote/1.0",
            "external-auth-user");

        using var context = TestContext.Create(httpContext: httpContext);
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.TenantId, space.Id);
        var answer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.TenantId,
            question.Id,
            "Public accepted answer",
            accept: true);
        var voteHandler = CreateVoteHandler(context);

        var voteId = await voteHandler.Handle(new VotesCreateVoteCommand
        {
            Request = new AnswerVoteCreateRequestDto
            {
                QuestionId = question.Id,
                AnswerId = answer.Id,
                IsUpvote = true
            }
        }, CancellationToken.None);

        var activity = await context.DbContext.Activities.FindAsync(voteId);
        var result = await CreateQuestionHandler(context).Handle(new QuestionsGetQuestionQuery
        {
            Id = question.Id,
            Request = new QuestionGetRequestDto()
        }, CancellationToken.None);

        Assert.NotNull(activity);
        Assert.Equal(context.UserId.ToString("D"), activity!.UserPrint);
        Assert.Equal("192.0.2.45", activity.Ip);
        Assert.Equal("QnAPublicVote/1.0", activity.UserAgent);
        Assert.Equal(context.UserId.ToString("D"), ActivitySignals.ParseVote(activity.MetadataJson)?.UserPrint);
        Assert.Equal(1, result.AcceptedAnswer!.VoteScore);
    }

    [Fact]
    public async Task CreateVote_UsesFingerprintForAnonymousRequests()
    {
        var httpContext = CreateHttpContext("192.0.2.46", "QnAPublicVote/2.0");

        using var context = TestContext.Create(httpContext: httpContext);
        var space = await TestDataFactory.SeedSpaceAsync(context.DbContext, context.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.TenantId, space.Id);
        var answer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.TenantId,
            question.Id,
            "Public accepted answer",
            accept: true);
        var expectedIdentity = ActivityIdentityResolver.ResolveActivityIdentity(
            context.SessionService,
            ActivityRequestInfo.GetRequiredIp(context.HttpContextAccessor.HttpContext!),
            ActivityRequestInfo.GetRequiredUserAgent(context.HttpContextAccessor.HttpContext!),
            new ClaimService(context.HttpContextAccessor).GetExternalUserId());

        var voteId = await CreateVoteHandler(context).Handle(new VotesCreateVoteCommand
        {
            Request = new AnswerVoteCreateRequestDto
            {
                QuestionId = question.Id,
                AnswerId = answer.Id,
                IsUpvote = true
            }
        }, CancellationToken.None);

        var activity = await context.DbContext.Activities.FindAsync(voteId);

        Assert.NotNull(activity);
        Assert.Equal(expectedIdentity.UserPrint, activity!.UserPrint);
        Assert.Equal(expectedIdentity.Ip, activity.Ip);
        Assert.Equal(expectedIdentity.UserAgent, activity.UserAgent);
        Assert.Equal(expectedIdentity.UserPrint, ActivitySignals.ParseVote(activity.MetadataJson)?.UserPrint);
    }

    private static VotesCreateVoteCommandHandler CreateVoteHandler(TestContext context)
    {
        return new VotesCreateVoteCommandHandler(
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
            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    [new Claim("sub", externalUserId)],
                    "TestAuth"));

        return httpContext;
    }
}