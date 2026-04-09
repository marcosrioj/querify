using System.Net;
using BaseFaq.Faq.Portal.Business.Faq.Commands.CreateFaqContentRef;
using BaseFaq.Faq.Portal.Business.Faq.Commands.CreateFaqTag;
using BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqContentRef;
using BaseFaq.Faq.Portal.Business.Faq.Queries.GetFaqTag;
using BaseFaq.Faq.Portal.Business.FaqItem.Commands.CreateFaqItem;
using BaseFaq.Faq.Portal.Business.FaqItem.Queries.GetFaqItem;
using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Commands.CreateFaqItemAnswer;
using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Queries.GetFaqItemAnswer;
using BaseFaq.Faq.Portal.Business.Feedback.Commands.CreateFeedback;
using BaseFaq.Faq.Portal.Business.Feedback.Queries.GetFeedback;
using BaseFaq.Faq.Portal.Business.Vote.Commands.CreateVote;
using BaseFaq.Faq.Portal.Business.Vote.Queries.GetVote;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Tests.Relationships;

public class RelationshipMappingTests
{
    [Fact]
    public async Task FaqItem_Query_ReturnsContentRefRelationship()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);

        var createHandler = new FaqItemsCreateFaqItemCommandHandler(context.DbContext, context.SessionService);
        var createRequest = new FaqItemsCreateFaqItemCommand
        {
            Question = "Question",
            Sort = 1,
            IsActive = true,
            FaqId = faq.Id,
            ContentRefId = contentRef.Id
        };

        var id = await createHandler.Handle(createRequest, CancellationToken.None);

        var queryHandler = new FaqItemsGetFaqItemQueryHandler(context.DbContext);
        var result = await queryHandler.Handle(new FaqItemsGetFaqItemQuery { Id = id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faq.Id, result!.FaqId);
        Assert.Equal(contentRef.Id, result.ContentRefId);
    }

    [Fact]
    public async Task FaqTag_Query_ReturnsFaqAndTagRelationship()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var tag = await TestDataFactory.SeedTagAsync(context.DbContext, context.SessionService.TenantId);

        var createHandler = new FaqTagsCreateFaqTagCommandHandler(context.DbContext, context.SessionService);
        var createRequest = new FaqTagsCreateFaqTagCommand { FaqId = faq.Id, TagId = tag.Id };

        var id = await createHandler.Handle(createRequest, CancellationToken.None);

        var queryHandler = new FaqTagsGetFaqTagQueryHandler(context.DbContext);
        var result = await queryHandler.Handle(new FaqTagsGetFaqTagQuery { Id = id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faq.Id, result!.FaqId);
        Assert.Equal(tag.Id, result.TagId);
    }

    [Fact]
    public async Task FaqContentRef_Query_ReturnsFaqAndContentRefRelationship()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);

        var createHandler = new FaqContentRefsCreateFaqContentRefCommandHandler(
            context.DbContext,
            context.SessionService);
        var createRequest = new FaqContentRefsCreateFaqContentRefCommand
        {
            FaqId = faq.Id,
            ContentRefId = contentRef.Id
        };

        var id = await createHandler.Handle(createRequest, CancellationToken.None);

        var queryHandler = new FaqContentRefsGetFaqContentRefQueryHandler(context.DbContext);
        var result = await queryHandler.Handle(
            new FaqContentRefsGetFaqContentRefQuery { Id = id },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faq.Id, result!.FaqId);
        Assert.Equal(contentRef.Id, result.ContentRefId);
    }

    [Fact]
    public async Task Feedback_Query_ReturnsFaqItemRelationship()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.12");
        httpContext.Request.Headers.UserAgent = "RelAgent/1.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);

        var createHandler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var createRequest = new FeedbacksCreateFeedbackCommand
        {
            Like = true,
            UnLikeReason = null,
            FaqItemId = faqItem.Id
        };

        var id = await createHandler.Handle(createRequest, CancellationToken.None);

        var queryHandler = new FeedbacksGetFeedbackQueryHandler(context.DbContext);
        var result = await queryHandler.Handle(new FeedbacksGetFeedbackQuery { Id = id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faqItem.Id, result!.FaqItemId);
    }

    [Fact]
    public async Task FaqItemAnswer_Query_ReturnsFaqItemRelationship()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);

        var createHandler = new FaqItemAnswersCreateFaqItemAnswerCommandHandler(
            context.DbContext,
            context.SessionService);
        var createRequest = new FaqItemAnswersCreateFaqItemAnswerCommand
        {
            ShortAnswer = "Alternative short",
            Answer = "Alternative answer",
            Sort = 2,
            IsActive = true,
            FaqItemId = faqItem.Id
        };

        var id = await createHandler.Handle(createRequest, CancellationToken.None);

        var queryHandler = new FaqItemAnswersGetFaqItemAnswerQueryHandler(context.DbContext);
        var result = await queryHandler.Handle(
            new FaqItemAnswersGetFaqItemAnswerQuery { Id = id },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faqItem.Id, result!.FaqItemId);
    }

    [Fact]
    public async Task Vote_Query_ReturnsFaqItemAnswerRelationship()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.21");
        httpContext.Request.Headers.UserAgent = "VoteRelAgent/1.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);
        var faqItemAnswer = await TestDataFactory.SeedFaqItemAnswerAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id,
            shortAnswer: "Second option",
            answer: "Detailed second option",
            sort: 2);

        var createHandler = new VotesCreateVoteCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var id = await createHandler.Handle(new VotesCreateVoteCommand
        {
            FaqItemAnswerId = faqItemAnswer.Id
        }, CancellationToken.None);

        var queryHandler = new VotesGetVoteQueryHandler(context.DbContext);
        var result = await queryHandler.Handle(new VotesGetVoteQuery { Id = id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faqItemAnswer.Id, result!.FaqItemAnswerId);
    }
}
