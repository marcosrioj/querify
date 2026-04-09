using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Portal.Business.Feedback.Commands.CreateFeedback;
using BaseFaq.Faq.Portal.Business.Feedback.Commands.DeleteFeedback;
using BaseFaq.Faq.Portal.Business.Feedback.Commands.UpdateFeedback;
using BaseFaq.Faq.Portal.Business.Feedback.Helpers;
using BaseFaq.Faq.Portal.Business.Feedback.Queries.GetFeedback;
using BaseFaq.Faq.Portal.Business.Feedback.Queries.GetFeedbackList;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Faq.Dtos.Feedback;
using BaseFaq.Models.Faq.Enums;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Tests.Feedback;

public class FeedbackCommandQueryTests
{
    [Fact]
    public async Task CreateFeedback_PersistsEntityAndReturnsId()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.44");
        httpContext.Request.Headers.UserAgent = "TestAgent/1.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);

        var handler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var request = new FeedbacksCreateFeedbackCommand
        {
            Like = true,
            UnLikeReason = null,
            FaqItemId = faqItem.Id
        };

        var identity = FeedbackRequestContext.GetIdentity(context.SessionService, context.HttpContextAccessor);
        var id = await handler.Handle(request, CancellationToken.None);

        var feedback = await context.DbContext.Feedbacks.FindAsync(id);
        Assert.NotNull(feedback);
        Assert.True(feedback!.Like);
        Assert.Equal(identity.UserPrint, feedback.UserPrint);
        Assert.Equal(identity.Ip, feedback.Ip);
        Assert.Equal(identity.UserAgent, feedback.UserAgent);
        Assert.Equal(faqItem.Id, feedback.FaqItemId);
        Assert.Equal(context.SessionService.TenantId, feedback.TenantId);
    }

    [Fact]
    public async Task CreateFeedback_ThrowsWhenUnLikeReasonMissing()
    {
        using var context = TestContext.Create(httpContext: new DefaultHttpContext());
        var handler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var request = new FeedbacksCreateFeedbackCommand
        {
            Like = false,
            UnLikeReason = null,
            FaqItemId = Guid.NewGuid()
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(422, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateFeedback_UsesAuthenticatedUserIdForUserPrint()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.100");
        httpContext.Request.Headers.UserAgent = "AuthAgent/1.0";
        httpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity("TestAuth"));

        var userId = Guid.NewGuid();
        using var context = TestContext.Create(userId: userId, httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);

        var handler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var request = new FeedbacksCreateFeedbackCommand
        {
            Like = true,
            UnLikeReason = null,
            FaqItemId = faqItem.Id
        };

        var id = await handler.Handle(request, CancellationToken.None);
        var feedback = await context.DbContext.Feedbacks.FindAsync(id);

        Assert.NotNull(feedback);
        Assert.Equal(userId.ToString(), feedback!.UserPrint);
    }

    [Fact]
    public async Task CreateFeedback_ThrowsWhenHttpContextMissing()
    {
        using var context = TestContext.Create(httpContext: null);
        var handler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var request = new FeedbacksCreateFeedbackCommand
        {
            Like = true,
            UnLikeReason = null,
            FaqItemId = Guid.NewGuid()
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(401, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateFeedback_UpdatesExistingFeedback()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("198.51.100.10");
        httpContext.Request.Headers.UserAgent = "UpdateAgent/2.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);
        var feedback = await TestDataFactory.SeedFeedbackAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id,
            like: true);

        var handler = new FeedbacksUpdateFeedbackCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var request = new FeedbacksUpdateFeedbackCommand
        {
            Id = feedback.Id,
            Like = false,
            UnLikeReason = UnLikeReason.LengthIssue,
            FaqItemId = faqItem.Id
        };

        var identity = FeedbackRequestContext.GetIdentity(context.SessionService, context.HttpContextAccessor);
        await handler.Handle(request, CancellationToken.None);

        var updated = await context.DbContext.Feedbacks.FindAsync(feedback.Id);
        Assert.NotNull(updated);
        Assert.False(updated!.Like);
        Assert.Equal(UnLikeReason.LengthIssue, updated.UnLikeReason);
        Assert.Equal(identity.UserPrint, updated.UserPrint);
        Assert.Equal(identity.Ip, updated.Ip);
        Assert.Equal(identity.UserAgent, updated.UserAgent);
    }

    [Fact]
    public async Task UpdateFeedback_ThrowsWhenUnLikeReasonMissing()
    {
        using var context = TestContext.Create(httpContext: new DefaultHttpContext());
        var handler = new FeedbacksUpdateFeedbackCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var request = new FeedbacksUpdateFeedbackCommand
        {
            Id = Guid.NewGuid(),
            Like = false,
            UnLikeReason = null,
            FaqItemId = Guid.NewGuid()
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(400, exception.ErrorCode);
    }

    [Fact]
    public async Task UpdateFeedback_ThrowsWhenMissing()
    {
        using var context = TestContext.Create(httpContext: new DefaultHttpContext());
        var handler = new FeedbacksUpdateFeedbackCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var request = new FeedbacksUpdateFeedbackCommand
        {
            Id = Guid.NewGuid(),
            Like = true,
            UnLikeReason = null,
            FaqItemId = Guid.NewGuid()
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task DeleteFeedback_SoftDeletesEntity()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);
        var feedback = await TestDataFactory.SeedFeedbackAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id);

        var handler = new FeedbacksDeleteFeedbackCommandHandler(context.DbContext);
        await handler.Handle(new FeedbacksDeleteFeedbackCommand { Id = feedback.Id }, CancellationToken.None);

        context.DbContext.SoftDeleteFiltersEnabled = false;
        var deleted = await context.DbContext.Feedbacks.FindAsync(feedback.Id);
        Assert.NotNull(deleted);
        Assert.True(deleted!.IsDeleted);
    }

    [Fact]
    public async Task GetFeedback_ReturnsDto()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);
        var feedback = await TestDataFactory.SeedFeedbackAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id);

        var handler = new FeedbacksGetFeedbackQueryHandler(context.DbContext);
        var result = await handler.Handle(new FeedbacksGetFeedbackQuery { Id = feedback.Id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(feedback.Id, result!.Id);
        Assert.Equal(feedback.Like, result.Like);
        Assert.Equal(feedback.UserPrint, result.UserPrint);
        Assert.Equal(feedback.UnLikeReason, result.UnLikeReason);
        Assert.Equal(feedback.FaqItemId, result.FaqItemId);
    }

    [Fact]
    public async Task GetFeedbackList_ReturnsPagedItems()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);
        await TestDataFactory.SeedFeedbackAsync(context.DbContext, context.SessionService.TenantId, faqItem.Id);
        await TestDataFactory.SeedFeedbackAsync(context.DbContext, context.SessionService.TenantId, faqItem.Id);

        var handler = new FeedbacksGetFeedbackListQueryHandler(context.DbContext);
        var request = new FeedbacksGetFeedbackListQuery
        {
            Request = new FeedbackGetAllRequestDto { SkipCount = 0, MaxResultCount = 10 }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task CreateFeedback_UsesForwardedForIpWhenProvided()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.10");
        httpContext.Request.Headers.UserAgent = "ForwardAgent/1.0";
        httpContext.Request.Headers["X-Forwarded-For"] = "203.0.113.8, 70.41.3.18";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);

        var handler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var request = new FeedbacksCreateFeedbackCommand
        {
            Like = true,
            UnLikeReason = null,
            FaqItemId = faqItem.Id
        };

        var identity = FeedbackRequestContext.GetIdentity(context.SessionService, context.HttpContextAccessor);
        var id = await handler.Handle(request, CancellationToken.None);

        var feedback = await context.DbContext.Feedbacks.FindAsync(id);
        Assert.NotNull(feedback);
        Assert.Equal("203.0.113.8", feedback!.Ip);
        Assert.Equal(identity.UserPrint, feedback.UserPrint);
    }

    [Fact]
    public async Task UpdateFeedback_ThrowsWhenHttpContextMissing()
    {
        using var context = TestContext.Create(httpContext: null);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);
        var feedback = await TestDataFactory.SeedFeedbackAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id);

        var handler = new FeedbacksUpdateFeedbackCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var request = new FeedbacksUpdateFeedbackCommand
        {
            Id = feedback.Id,
            Like = true,
            UnLikeReason = null,
            FaqItemId = faqItem.Id
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(401, exception.ErrorCode);
    }

    [Fact]
    public async Task GetFeedbackList_SortsByExplicitField()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);
        await TestDataFactory.SeedFeedbackAsync(context.DbContext, context.SessionService.TenantId, faqItem.Id,
            like: false);
        await TestDataFactory.SeedFeedbackAsync(context.DbContext, context.SessionService.TenantId, faqItem.Id, like: true);

        var handler = new FeedbacksGetFeedbackListQueryHandler(context.DbContext);
        var request = new FeedbacksGetFeedbackListQuery
        {
            Request = new FeedbackGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "like DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.True(result.Items[0].Like);
        Assert.False(result.Items[1].Like);
    }

    [Fact]
    public async Task GetFeedbackList_FallsBackToUpdatedDateWhenSortingInvalid()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);
        var first = await TestDataFactory.SeedFeedbackAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id);
        var second = await TestDataFactory.SeedFeedbackAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faqItem.Id);
        second.UserPrint = "updated-user-print";
        await context.DbContext.SaveChangesAsync();

        var handler = new FeedbacksGetFeedbackListQueryHandler(context.DbContext);
        var request = new FeedbacksGetFeedbackListQuery
        {
            Request = new FeedbackGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "unknown DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(second.Id, result.Items[0].Id);
        Assert.Equal(first.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task GetFeedbackList_SortsByMultipleFields()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);

        var feedbackA = new Common.Persistence.FaqDb.Entities.Feedback
        {
            Like = true,
            UserPrint = "b-user",
            Ip = "127.0.0.1",
            UserAgent = "agent",
            UnLikeReason = null,
            TenantId = context.SessionService.TenantId,
            FaqItemId = faqItem.Id
        };
        var feedbackB = new Common.Persistence.FaqDb.Entities.Feedback
        {
            Like = true,
            UserPrint = "a-user",
            Ip = "127.0.0.1",
            UserAgent = "agent",
            UnLikeReason = null,
            TenantId = context.SessionService.TenantId,
            FaqItemId = faqItem.Id
        };
        var feedbackC = new Common.Persistence.FaqDb.Entities.Feedback
        {
            Like = false,
            UserPrint = "z-user",
            Ip = "127.0.0.1",
            UserAgent = "agent",
            UnLikeReason = UnLikeReason.NotRelevant,
            TenantId = context.SessionService.TenantId,
            FaqItemId = faqItem.Id
        };

        context.DbContext.Feedbacks.AddRange(feedbackA, feedbackB, feedbackC);
        await context.DbContext.SaveChangesAsync();

        var handler = new FeedbacksGetFeedbackListQueryHandler(context.DbContext);
        var request = new FeedbacksGetFeedbackListQuery
        {
            Request = new FeedbackGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "like DESC, userprint ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(feedbackB.Id, result.Items[0].Id);
        Assert.Equal(feedbackA.Id, result.Items[1].Id);
        Assert.Equal(feedbackC.Id, result.Items[2].Id);
    }

    [Fact]
    public async Task CreateFeedback_AllowsMissingUserAgentAndIp()
    {
        var httpContext = new DefaultHttpContext();

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);

        var handler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            context.SessionService,
            context.HttpContextAccessor);
        var request = new FeedbacksCreateFeedbackCommand
        {
            Like = true,
            UnLikeReason = null,
            FaqItemId = faqItem.Id
        };

        var id = await handler.Handle(request, CancellationToken.None);
        var feedback = await context.DbContext.Feedbacks.FindAsync(id);

        Assert.NotNull(feedback);
        Assert.Equal(string.Empty, feedback!.Ip);
        Assert.Equal(string.Empty, feedback.UserAgent);
    }

    [Fact]
    public async Task GetFeedbackList_AppliesPaginationWindow()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem =
            await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.SessionService.TenantId, faq.Id);

        await TestDataFactory.SeedFeedbackAsync(context.DbContext, context.SessionService.TenantId, faqItem.Id, like: true);
        await TestDataFactory.SeedFeedbackAsync(context.DbContext, context.SessionService.TenantId, faqItem.Id,
            like: false);
        await TestDataFactory.SeedFeedbackAsync(context.DbContext, context.SessionService.TenantId, faqItem.Id, like: true);

        var handler = new FeedbacksGetFeedbackListQueryHandler(context.DbContext);
        var request = new FeedbacksGetFeedbackListQuery
        {
            Request = new FeedbackGetAllRequestDto
            {
                SkipCount = 1,
                MaxResultCount = 1,
                Sorting = "id ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
    }
}