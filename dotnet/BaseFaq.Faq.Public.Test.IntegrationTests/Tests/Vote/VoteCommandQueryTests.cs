using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Public.Business.Vote.Commands.CreateVote;
using BaseFaq.Faq.Public.Business.Vote.Helpers;
using BaseFaq.Faq.Public.Test.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BaseFaq.Faq.Public.Test.IntegrationTests.Tests.Vote;

public class VoteCommandQueryTests
{
    [Fact]
    public async Task CreateVote_PersistsEntityAndUpdatesVoteScore()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.99");
        httpContext.Request.Headers.UserAgent = "PublicVoteAgent/1.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.TenantId, faq.Id);
        var faqItemAnswer = await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.TenantId,
            faqItem.Id);

        var handler = new VotesCreateVoteCommandHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.HttpContextAccessor);
        var identity = VoteRequestContext.GetIdentity(context.HttpContextAccessor);
        var id = await handler.Handle(new VotesCreateVoteCommand
        {
            FaqItemAnswerId = faqItemAnswer.Id
        }, CancellationToken.None);

        var vote = await context.DbContext.Votes.FindAsync(id);
        var updatedAnswer = await context.DbContext.FaqItemAnswers.FindAsync(faqItemAnswer.Id);

        Assert.NotNull(vote);
        Assert.Equal(identity.UserPrint, vote!.UserPrint);
        Assert.Equal(identity.Ip, vote.Ip);
        Assert.Equal(identity.UserAgent, vote.UserAgent);
        Assert.Equal(faqItemAnswer.Id, vote.FaqItemAnswerId);
        Assert.NotNull(updatedAnswer);
        Assert.Equal(1, updatedAnswer!.VoteScore);
    }

    [Fact]
    public async Task CreateVote_TogglesVoteForSameUserPrint()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("198.51.100.30");
        httpContext.Request.Headers.UserAgent = "DuplicateVoteAgent/1.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.TenantId, faq.Id);
        var faqItemAnswer = await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.TenantId,
            faqItem.Id);

        var handler = new VotesCreateVoteCommandHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.HttpContextAccessor);

        var firstId = await handler.Handle(new VotesCreateVoteCommand
        {
            FaqItemAnswerId = faqItemAnswer.Id
        }, CancellationToken.None);

        // Second call from the same user toggles the vote off
        var secondId = await handler.Handle(new VotesCreateVoteCommand
        {
            FaqItemAnswerId = faqItemAnswer.Id
        }, CancellationToken.None);

        var votes = context.DbContext.Votes.Where(vote => vote.FaqItemAnswerId == faqItemAnswer.Id).ToList();
        var updatedAnswer = await context.DbContext.FaqItemAnswers.FindAsync(faqItemAnswer.Id);

        Assert.NotEqual(Guid.Empty, firstId);
        Assert.Equal(Guid.Empty, secondId);
        Assert.Empty(votes);
        Assert.Equal(0, updatedAnswer!.VoteScore);
    }

    [Fact]
    public async Task CreateVote_ThrowsWhenFaqItemAnswerDoesNotExist()
    {
        using var context = TestContext.Create(httpContext: new DefaultHttpContext());
        var handler = new VotesCreateVoteCommandHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.HttpContextAccessor);

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new VotesCreateVoteCommand { FaqItemAnswerId = Guid.NewGuid() },
            CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateVote_AllowsDifferentUsersToVoteOnSameAnswer()
    {
        var httpContextA = new DefaultHttpContext();
        httpContextA.Connection.RemoteIpAddress = IPAddress.Parse("198.51.100.31");
        httpContextA.Request.Headers.UserAgent = "VoteAgentA/1.0";

        using var database =
            global::BaseFaq.Faq.Public.Test.IntegrationTests.Helpers.Infrastructure.TestDatabase.Create();
        using var contextA = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            httpContext: httpContextA);
        var faq = await TestDataFactory.SeedFaqAsync(contextA.DbContext, contextA.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(contextA.DbContext, contextA.TenantId, faq.Id);
        var faqItemAnswer = await TestDataFactory.SeedFaqItemAnswerAsync(
            contextA.DbContext,
            contextA.TenantId,
            faqItem.Id);

        var handlerA = new VotesCreateVoteCommandHandler(
            contextA.DbContext,
            new TestClientKeyContextService(contextA.ClientKey),
            new TestTenantClientKeyResolver(contextA.TenantId, contextA.ClientKey),
            contextA.HttpContextAccessor);
        await handlerA.Handle(new VotesCreateVoteCommand
        {
            FaqItemAnswerId = faqItemAnswer.Id
        }, CancellationToken.None);

        var httpContextB = new DefaultHttpContext();
        httpContextB.Connection.RemoteIpAddress = IPAddress.Parse("198.51.100.32");
        httpContextB.Request.Headers.UserAgent = "VoteAgentB/1.0";

        using var contextB = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: contextA.TenantId,
            httpContext: httpContextB);
        var handlerB = new VotesCreateVoteCommandHandler(
            contextB.DbContext,
            new TestClientKeyContextService(contextB.ClientKey),
            new TestTenantClientKeyResolver(contextB.TenantId, contextB.ClientKey),
            contextB.HttpContextAccessor);
        await handlerB.Handle(new VotesCreateVoteCommand
        {
            FaqItemAnswerId = faqItemAnswer.Id
        }, CancellationToken.None);

        var votes = contextB.DbContext.Votes.Where(vote => vote.FaqItemAnswerId == faqItemAnswer.Id).ToList();
        var updatedAnswer = await contextB.DbContext.FaqItemAnswers.FindAsync(faqItemAnswer.Id);

        Assert.Equal(2, votes.Count);
        Assert.Equal(2, updatedAnswer!.VoteScore);
    }
}
