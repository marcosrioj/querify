using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Portal.Business.Vote.Commands.CreateVote;
using BaseFaq.Faq.Portal.Business.Vote.Commands.DeleteVote;
using BaseFaq.Faq.Portal.Business.Vote.Commands.UpdateVote;
using BaseFaq.Faq.Portal.Business.Vote.Helpers;
using BaseFaq.Faq.Portal.Business.Vote.Queries.GetVote;
using BaseFaq.Faq.Portal.Business.Vote.Queries.GetVoteList;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Faq.Dtos.Vote;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Tests.Vote;

public class VoteCommandQueryTests
{
    [Fact]
    public async Task CreateVote_PersistsEntityAndUpdatesVoteScore()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.88");
        httpContext.Request.Headers.UserAgent = "VoteAgent/1.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.SessionService.TenantId, faq.Id);
        var faqItemAnswer = await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id);

        var handler = new VotesCreateVoteCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var identity = VoteRequestContext.GetIdentity(context.SessionService, context.HttpContextAccessor);
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
    public async Task CreateVote_ThrowsWhenHttpContextMissing()
    {
        using var context = TestContext.Create(httpContext: null);
        var handler = new VotesCreateVoteCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);

        var exception = await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(
            new VotesCreateVoteCommand { FaqItemAnswerId = Guid.NewGuid() },
            CancellationToken.None));

        Assert.Equal(401, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateVote_ChangesAnswerAndRecalculatesVoteScores()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("198.51.100.55");
        httpContext.Request.Headers.UserAgent = "VoteUpdateAgent/1.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.SessionService.TenantId, faq.Id);
        var answerA = await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id,
            shortAnswer: "Answer A");
        var answerB = await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id,
            shortAnswer: "Answer B");
        var vote = await TestDataFactory.SeedVoteAsync(
            context.DbContext,
            context.SessionService.TenantId,
            answerA.Id);
        answerA.VoteScore = 1;
        await context.DbContext.SaveChangesAsync();

        var handler = new VotesUpdateVoteCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        await handler.Handle(new VotesUpdateVoteCommand
        {
            Id = vote.Id,
            FaqItemAnswerId = answerB.Id
        }, CancellationToken.None);

        var updatedVote = await context.DbContext.Votes.FindAsync(vote.Id);
        var updatedAnswerA = await context.DbContext.FaqItemAnswers.FindAsync(answerA.Id);
        var updatedAnswerB = await context.DbContext.FaqItemAnswers.FindAsync(answerB.Id);

        Assert.NotNull(updatedVote);
        Assert.Equal(answerB.Id, updatedVote!.FaqItemAnswerId);
        Assert.Equal(0, updatedAnswerA!.VoteScore);
        Assert.Equal(1, updatedAnswerB!.VoteScore);
    }

    [Fact]
    public async Task DeleteVote_SoftDeletesEntityAndUpdatesVoteScore()
    {
        using var context = TestContext.Create(httpContext: new DefaultHttpContext());
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.SessionService.TenantId, faq.Id);
        var faqItemAnswer = await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id,
            voteScore: 1);
        var vote = await TestDataFactory.SeedVoteAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItemAnswer.Id);

        var handler = new VotesDeleteVoteCommandHandler(context.DbContext);
        await handler.Handle(new VotesDeleteVoteCommand { Id = vote.Id }, CancellationToken.None);

        context.DbContext.SoftDeleteFiltersEnabled = false;
        var deletedVote = await context.DbContext.Votes.FindAsync(vote.Id);
        var updatedAnswer = await context.DbContext.FaqItemAnswers.FindAsync(faqItemAnswer.Id);

        Assert.NotNull(deletedVote);
        Assert.True(deletedVote!.IsDeleted);
        Assert.Equal(0, updatedAnswer!.VoteScore);
    }

    [Fact]
    public async Task GetVote_ReturnsDto()
    {
        using var context = TestContext.Create(httpContext: new DefaultHttpContext());
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.SessionService.TenantId, faq.Id);
        var faqItemAnswer = await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id);
        var vote = await TestDataFactory.SeedVoteAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItemAnswer.Id);

        var handler = new VotesGetVoteQueryHandler(context.DbContext);
        var result = await handler.Handle(new VotesGetVoteQuery { Id = vote.Id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(vote.Id, result!.Id);
        Assert.Equal(faqItemAnswer.Id, result.FaqItemAnswerId);
    }

    [Fact]
    public async Task GetVoteList_ReturnsPagedItems()
    {
        using var context = TestContext.Create(httpContext: new DefaultHttpContext());
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.SessionService.TenantId, faq.Id);
        var faqItemAnswer = await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id);
        await TestDataFactory.SeedVoteAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItemAnswer.Id,
            userPrint: "b-user");
        await TestDataFactory.SeedVoteAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItemAnswer.Id,
            userPrint: "a-user");

        var handler = new VotesGetVoteListQueryHandler(context.DbContext);
        var result = await handler.Handle(new VotesGetVoteListQuery
        {
            Request = new VoteGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                FaqItemAnswerId = faqItemAnswer.Id,
                Sorting = "userprint ASC"
            }
        }, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal("a-user", result.Items[0].UserPrint);
        Assert.Equal("b-user", result.Items[1].UserPrint);
    }
}
