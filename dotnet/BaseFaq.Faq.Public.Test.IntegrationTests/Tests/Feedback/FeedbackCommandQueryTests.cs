using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Public.Business.Feedback.Commands.CreateFeedback;
using BaseFaq.Faq.Public.Business.Feedback.Helpers;
using BaseFaq.Faq.Public.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Faq.Enums;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace BaseFaq.Faq.Public.Test.IntegrationTests.Tests.Feedback;

public class FeedbackCommandQueryTests
{
    [Fact]
    public async Task CreateFeedback_PersistsEntityAndUpdatesFeedbackScore()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.0.2.44");
        httpContext.Request.Headers.UserAgent = "TestAgent/1.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.TenantId,
            faq.Id);

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
        var request = new FeedbacksCreateFeedbackCommand
        {
            Like = true,
            UnLikeReason = null,
            FaqItemId = faqItem.Id
        };

        var identity = FeedbackRequestContext.GetIdentity(context.HttpContextAccessor);
        var id = await handler.Handle(request, CancellationToken.None);

        var feedback = await context.DbContext.Feedbacks.FindAsync(id);
        var updatedFaqItem = await context.DbContext.FaqItems.FindAsync(faqItem.Id);

        Assert.NotNull(feedback);
        Assert.True(feedback!.Like);
        Assert.Equal(identity.UserPrint, feedback.UserPrint);
        Assert.Equal(identity.Ip, feedback.Ip);
        Assert.Equal(identity.UserAgent, feedback.UserAgent);
        Assert.Equal(faqItem.Id, feedback.FaqItemId);
        Assert.Equal(context.TenantId, feedback.TenantId);
        Assert.NotNull(updatedFaqItem);
        Assert.Equal(1, updatedFaqItem!.FeedbackScore);
    }

    [Fact]
    public async Task CreateFeedback_UpdatesExistingFeedbackByUserPrint()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("198.51.100.10");
        httpContext.Request.Headers.UserAgent = "DupAgent/2.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.TenantId,
            faq.Id);

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
        var firstRequest = new FeedbacksCreateFeedbackCommand
        {
            Like = true,
            UnLikeReason = null,
            FaqItemId = faqItem.Id
        };

        var secondRequest = new FeedbacksCreateFeedbackCommand
        {
            Like = false,
            UnLikeReason = UnLikeReason.NotRelevant,
            FaqItemId = faqItem.Id
        };

        var firstId = await handler.Handle(firstRequest, CancellationToken.None);
        var secondId = await handler.Handle(secondRequest, CancellationToken.None);

        var feedbacks = context.DbContext.Feedbacks.Where(v => v.FaqItemId == faqItem.Id).ToList();
        var updatedFaqItem = await context.DbContext.FaqItems.FindAsync(faqItem.Id);

        Assert.Single(feedbacks);
        Assert.Equal(firstId, secondId);
        Assert.False(feedbacks[0].Like);
        Assert.Equal(UnLikeReason.NotRelevant, feedbacks[0].UnLikeReason);
        Assert.NotNull(updatedFaqItem);
        Assert.Equal(-1, updatedFaqItem!.FeedbackScore);
    }

    [Fact]
    public async Task CreateFeedback_DoesNothingWhenExistingLikeIsUnchanged()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("198.51.100.11");
        httpContext.Request.Headers.UserAgent = "SameLikeAgent/1.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.TenantId,
            faq.Id);

        var handler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            new TestClientKeyContextService(context.ClientKey),
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey),
            context.HttpContextAccessor);

        var firstId = await handler.Handle(new FeedbacksCreateFeedbackCommand
        {
            Like = false,
            UnLikeReason = UnLikeReason.ConfusingOrUnclear,
            FaqItemId = faqItem.Id
        }, CancellationToken.None);

        var secondId = await handler.Handle(new FeedbacksCreateFeedbackCommand
        {
            Like = false,
            UnLikeReason = UnLikeReason.NotRelevant,
            FaqItemId = faqItem.Id
        }, CancellationToken.None);

        var feedback = await context.DbContext.Feedbacks.FindAsync(firstId);
        var updatedFaqItem = await context.DbContext.FaqItems.FindAsync(faqItem.Id);

        Assert.NotNull(feedback);
        Assert.Equal(firstId, secondId);
        Assert.False(feedback!.Like);
        Assert.Equal(UnLikeReason.ConfusingOrUnclear, feedback.UnLikeReason);
        Assert.NotNull(updatedFaqItem);
        Assert.Equal(-1, updatedFaqItem!.FeedbackScore);
    }

    [Fact]
    public async Task CreateFeedback_ThrowsWhenUnLikeReasonMissing()
    {
        using var context = TestContext.Create(httpContext: new DefaultHttpContext());
        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
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
    public async Task CreateFeedback_DislikeUpdatesScore()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.55");
        httpContext.Request.Headers.UserAgent = "DislikeAgent/1.0";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.TenantId,
            faq.Id);

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
        var request = new FeedbacksCreateFeedbackCommand
        {
            Like = false,
            UnLikeReason = UnLikeReason.NotRelevant,
            FaqItemId = faqItem.Id
        };

        await handler.Handle(request, CancellationToken.None);

        var updatedFaqItem = await context.DbContext.FaqItems.FindAsync(faqItem.Id);
        Assert.NotNull(updatedFaqItem);
        Assert.Equal(-1, updatedFaqItem!.FeedbackScore);
    }

    [Fact]
    public async Task CreateFeedback_ThrowsWhenFaqItemDoesNotExist()
    {
        using var context = TestContext.Create(httpContext: new DefaultHttpContext());
        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
        var request = new FeedbacksCreateFeedbackCommand
        {
            Like = true,
            FaqItemId = Guid.NewGuid()
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task CreateFeedback_AllowsDifferentUsersToFeedbackOnSameFaqItem()
    {
        var httpContextA = new DefaultHttpContext();
        httpContextA.Connection.RemoteIpAddress = IPAddress.Parse("198.51.100.20");
        httpContextA.Request.Headers.UserAgent = "FeedbackAgentA/1.0";

        using var database =
            global::BaseFaq.Faq.Public.Test.IntegrationTests.Helpers.Infrastructure.TestDatabase.Create();
        using var contextA = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            httpContext: httpContextA);
        var faq = await TestDataFactory.SeedFaqAsync(contextA.DbContext, contextA.TenantId);
        var faqItem =
            await TestDataFactory.SeedFaqItemAsync(contextA.DbContext, contextA.TenantId, faq.Id);

        var handlerA = new FeedbacksCreateFeedbackCommandHandler(
            contextA.DbContext,
            new TestClientKeyContextService(contextA.ClientKey),
            new TestTenantClientKeyResolver(contextA.TenantId, contextA.ClientKey),
            contextA.HttpContextAccessor);
        await handlerA.Handle(new FeedbacksCreateFeedbackCommand
        {
            Like = true,
            FaqItemId = faqItem.Id
        }, CancellationToken.None);

        var httpContextB = new DefaultHttpContext();
        httpContextB.Connection.RemoteIpAddress = IPAddress.Parse("198.51.100.21");
        httpContextB.Request.Headers.UserAgent = "FeedbackAgentB/1.0";

        using var contextB = TestContext.CreateForDatabase(
            database.ConnectionString,
            database.AdminConnectionString,
            database.DatabaseName,
            tenantId: contextA.TenantId,
            httpContext: httpContextB);
        var handlerB = new FeedbacksCreateFeedbackCommandHandler(
            contextB.DbContext,
            new TestClientKeyContextService(contextB.ClientKey),
            new TestTenantClientKeyResolver(contextB.TenantId, contextB.ClientKey),
            contextB.HttpContextAccessor);
        await handlerB.Handle(new FeedbacksCreateFeedbackCommand
        {
            Like = true,
            FaqItemId = faqItem.Id
        }, CancellationToken.None);

        var feedbacks = contextB.DbContext.Feedbacks.Where(v => v.FaqItemId == faqItem.Id).ToList();
        var updatedFaqItem = await contextB.DbContext.FaqItems.FindAsync(faqItem.Id);
        Assert.Equal(2, feedbacks.Count);
        Assert.NotNull(updatedFaqItem);
        Assert.Equal(2, updatedFaqItem!.FeedbackScore);
    }

    [Fact]
    public async Task CreateFeedback_UsesForwardedForIpWhenProvided()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.10");
        httpContext.Request.Headers.UserAgent = "ForwardAgent/1.0";
        httpContext.Request.Headers["X-Forwarded-For"] = "203.0.113.9, 70.41.3.18";

        using var context = TestContext.Create(httpContext: httpContext);
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.TenantId,
            faq.Id);

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FeedbacksCreateFeedbackCommandHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
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
        Assert.Equal("203.0.113.9", feedback!.Ip);
    }
}