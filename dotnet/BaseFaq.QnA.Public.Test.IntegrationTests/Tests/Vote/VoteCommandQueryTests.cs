using System.Net;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.QnA.Public.Business.Question.Queries.GetQuestion;
using BaseFaq.QnA.Public.Business.Vote.Commands.CreateVote;
using BaseFaq.QnA.Public.Test.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BaseFaq.QnA.Public.Test.IntegrationTests.Tests.Vote;

public class VoteCommandQueryTests
{
    [Fact]
    public async Task CreateVote_TogglesLatestVoteStateForAnswerScore()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.45");
        httpContext.Request.Headers.UserAgent = "QnAPublicVote/1.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var space = await TestDataFactory.SeedQuestionSpaceAsync(context.DbContext, context.TenantId);
        var question = await TestDataFactory.SeedQuestionAsync(context.DbContext, context.TenantId, space.Id);
        var answer = await TestDataFactory.SeedAnswerAsync(
            context.DbContext,
            context.TenantId,
            question.Id,
            "Public accepted answer",
            accept: true);
        var voteHandler = new VotesCreateVoteCommandHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.HttpContextAccessor);

        var firstVoteId = await voteHandler.Handle(new VotesCreateVoteCommand
        {
            Request = new AnswerVoteCreateRequestDto
            {
                QuestionId = question.Id,
                AnswerId = answer.Id,
                IsUpvote = true
            }
        }, CancellationToken.None);

        var secondVoteId = await voteHandler.Handle(new VotesCreateVoteCommand
        {
            Request = new AnswerVoteCreateRequestDto
            {
                QuestionId = question.Id,
                AnswerId = answer.Id,
                IsUpvote = true
            }
        }, CancellationToken.None);

        var questionHandler = new QuestionsGetQuestionQueryHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.HttpContextAccessor);
        var result = await questionHandler.Handle(new QuestionsGetQuestionQuery
        {
            Id = question.Id,
            Request = new QuestionGetRequestDto()
        }, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, firstVoteId);
        Assert.Equal(Guid.Empty, secondVoteId);
        Assert.NotNull(result.AcceptedAnswer);
        Assert.Equal(0, result.AcceptedAnswer!.VoteScore);
    }
}