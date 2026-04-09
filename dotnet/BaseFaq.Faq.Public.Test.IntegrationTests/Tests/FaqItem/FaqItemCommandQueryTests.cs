using BaseFaq.Faq.Public.Business.FaqItem.Commands.CreateFaqItem;
using BaseFaq.Faq.Public.Business.FaqItem.Queries.SearchFaqItem;
using BaseFaq.Faq.Public.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Ai.Contracts.Matching;
using BaseFaq.Models.Faq.Dtos.FaqItem;
using MassTransit;
using Moq;
using Xunit;

namespace BaseFaq.Faq.Public.Test.IntegrationTests.Tests.FaqItem;

public class FaqItemCommandQueryTests
{
    [Fact]
    public async Task CreateFaqItem_PersistsEntityAndReturnsId()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var tenantAiProviderResolver = new TestTenantAiProviderResolver();
        var publishEndpoint = new Mock<IPublishEndpoint>().Object;
        var handler = new FaqItemsCreateFaqItemCommandHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            tenantAiProviderResolver,
            context.HttpContextAccessor,
            publishEndpoint);
        var request = new FaqItemsCreateFaqItemCommand
        {
            Question = "How to sign in?",
            ShortAnswer = "Use your email.",
            Answer = "Sign in with email and password.",
            AdditionalInfo = "Contact support if needed.",
            CtaTitle = "Sign in",
            CtaUrl = "https://example.test/login",
            Sort = 2,
            IsActive = true,
            FaqId = faq.Id,
            ContentRefId = null
        };

        var id = await handler.Handle(request, CancellationToken.None);

        var faqItem = await context.DbContext.FaqItems.FindAsync(id);
        Assert.NotNull(faqItem);
        Assert.Equal("How to sign in?", faqItem!.Question);
        Assert.Equal(faq.Id, faqItem.FaqId);
        Assert.Equal(context.TenantId, faqItem.TenantId);
    }

    [Fact]
    public async Task CreateFaqItem_PublishesMatchingEvent_WhenTenantHasMatchingProvider()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver = new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var tenantAiProviderResolver = new TestTenantAiProviderResolver(hasProvider: true);
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var handler = new FaqItemsCreateFaqItemCommandHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            tenantAiProviderResolver,
            context.HttpContextAccessor,
            publishEndpoint.Object);

        await handler.Handle(new FaqItemsCreateFaqItemCommand
        {
            Question = "How to reset password?",
            ShortAnswer = "Use reset form.",
            Answer = "Use reset form.",
            AdditionalInfo = "N/A",
            CtaTitle = "Reset",
            CtaUrl = "https://example.test/reset",
            Sort = 1,
            IsActive = true,
            FaqId = faq.Id
        }, CancellationToken.None);

        publishEndpoint.Verify(
            x => x.Publish(
                It.IsAny<FaqMatchingRequestedV1>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateFaqItem_DoesNotPublishMatchingEvent_WhenTenantHasNoMatchingProvider()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver = new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var tenantAiProviderResolver = new TestTenantAiProviderResolver(hasProvider: false);
        var publishEndpoint = new Mock<IPublishEndpoint>();
        var handler = new FaqItemsCreateFaqItemCommandHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            tenantAiProviderResolver,
            context.HttpContextAccessor,
            publishEndpoint.Object);

        await handler.Handle(new FaqItemsCreateFaqItemCommand
        {
            Question = "How to update email?",
            ShortAnswer = "Use profile settings.",
            Answer = "Use profile settings.",
            AdditionalInfo = "N/A",
            CtaTitle = "Profile",
            CtaUrl = "https://example.test/profile",
            Sort = 1,
            IsActive = true,
            FaqId = faq.Id
        }, CancellationToken.None);

        publishEndpoint.Verify(
            x => x.Publish(
                It.IsAny<FaqMatchingRequestedV1>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchFaqItems_OrdersByDefaultSort()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);

        var first = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "First sort",
            ShortAnswer = "First",
            Answer = "First",
            AdditionalInfo = "First",
            CtaTitle = "First",
            CtaUrl = "https://example.test/first",
            Sort = 1,
            FeedbackScore = 5,
            AiConfidenceScore = 10,
            IsActive = true,
            FaqId = faq.Id,
            TenantId = context.TenantId
        };
        var second = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "Second sort",
            ShortAnswer = "Second",
            Answer = "Second",
            AdditionalInfo = "Second",
            CtaTitle = "Second",
            CtaUrl = "https://example.test/second",
            Sort = 2,
            FeedbackScore = 20,
            AiConfidenceScore = 50,
            IsActive = true,
            FaqId = faq.Id,
            TenantId = context.TenantId
        };

        context.DbContext.FaqItems.AddRange(first, second);
        await context.DbContext.SaveChangesAsync();

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FaqItemsSearchFaqItemQueryHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
        var request = new FaqItemsSearchFaqItemQuery
        {
            Request = new FaqItemSearchRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                FaqIds = [faq.Id]
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(first.Id, result.Items[0].Id);
        Assert.Equal(second.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task SearchFaqItems_FiltersByFaqIds()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);
        var otherFaq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId, "Other");
        await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.TenantId, faq.Id);
        await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.TenantId, otherFaq.Id);

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FaqItemsSearchFaqItemQueryHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
        var request = new FaqItemsSearchFaqItemQuery
        {
            Request = new FaqItemSearchRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                FaqIds = [faq.Id]
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(faq.Id, result.Items[0].FaqId);
    }

    [Fact]
    public async Task SearchFaqItems_FiltersBySearchTermInQuestion()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);

        context.DbContext.FaqItems.AddRange(
            new Common.Persistence.FaqDb.Entities.FaqItem
            {
                Question = "How to reset MFA?",
                ShortAnswer = "Use security page",
                Answer = "Go to security settings",
                AdditionalInfo = "Contact support if locked out",
                CtaTitle = "Security",
                CtaUrl = "https://example.test/security",
                Sort = 1,
                FeedbackScore = 0,
                AiConfidenceScore = 0,
                IsActive = true,
                FaqId = faq.Id,
                TenantId = context.TenantId
            },
            new Common.Persistence.FaqDb.Entities.FaqItem
            {
                Question = "How to change billing address?",
                ShortAnswer = "Use billing page",
                Answer = "Open billing settings",
                AdditionalInfo = "Requires owner role",
                CtaTitle = "Billing",
                CtaUrl = "https://example.test/billing",
                Sort = 2,
                FeedbackScore = 0,
                AiConfidenceScore = 0,
                IsActive = true,
                FaqId = faq.Id,
                TenantId = context.TenantId
            });
        await context.DbContext.SaveChangesAsync();

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FaqItemsSearchFaqItemQueryHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
        var request = new FaqItemsSearchFaqItemQuery
        {
            Request = new FaqItemSearchRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Search = "reset",
                FaqIds = [faq.Id]
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Contains("reset", result.Items[0].Question, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SearchFaqItems_AppliesPaginationAfterSort()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);

        context.DbContext.FaqItems.AddRange(
            new Common.Persistence.FaqDb.Entities.FaqItem
            {
                Question = "Q1",
                ShortAnswer = "A1",
                Answer = "A1",
                AdditionalInfo = "I1",
                CtaTitle = "C1",
                CtaUrl = "https://example.test/1",
                Sort = 1,
                FeedbackScore = 0,
                AiConfidenceScore = 0,
                IsActive = true,
                FaqId = faq.Id,
                TenantId = context.TenantId
            },
            new Common.Persistence.FaqDb.Entities.FaqItem
            {
                Question = "Q2",
                ShortAnswer = "A2",
                Answer = "A2",
                AdditionalInfo = "I2",
                CtaTitle = "C2",
                CtaUrl = "https://example.test/2",
                Sort = 2,
                FeedbackScore = 0,
                AiConfidenceScore = 0,
                IsActive = true,
                FaqId = faq.Id,
                TenantId = context.TenantId
            },
            new Common.Persistence.FaqDb.Entities.FaqItem
            {
                Question = "Q3",
                ShortAnswer = "A3",
                Answer = "A3",
                AdditionalInfo = "I3",
                CtaTitle = "C3",
                CtaUrl = "https://example.test/3",
                Sort = 3,
                FeedbackScore = 0,
                AiConfidenceScore = 0,
                IsActive = true,
                FaqId = faq.Id,
                TenantId = context.TenantId
            });
        await context.DbContext.SaveChangesAsync();

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FaqItemsSearchFaqItemQueryHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
        var request = new FaqItemsSearchFaqItemQuery
        {
            Request = new FaqItemSearchRequestDto
            {
                SkipCount = 1,
                MaxResultCount = 1,
                FaqIds = [faq.Id]
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Q2", result.Items[0].Question);
    }

    [Fact]
    public async Task SearchFaqItems_FiltersBySearchTermInAdditionalInfo()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);

        context.DbContext.FaqItems.AddRange(
            new Common.Persistence.FaqDb.Entities.FaqItem
            {
                Question = "General question",
                ShortAnswer = "General short",
                Answer = "General answer",
                AdditionalInfo = "Contains custom token: bluebird",
                CtaTitle = "General",
                CtaUrl = "https://example.test/general",
                Sort = 1,
                FeedbackScore = 0,
                AiConfidenceScore = 0,
                IsActive = true,
                FaqId = faq.Id,
                TenantId = context.TenantId
            },
            new Common.Persistence.FaqDb.Entities.FaqItem
            {
                Question = "Another question",
                ShortAnswer = "Another short",
                Answer = "Another answer",
                AdditionalInfo = "No token here",
                CtaTitle = "Another",
                CtaUrl = "https://example.test/another",
                Sort = 2,
                FeedbackScore = 0,
                AiConfidenceScore = 0,
                IsActive = true,
                FaqId = faq.Id,
                TenantId = context.TenantId
            });
        await context.DbContext.SaveChangesAsync();

        var clientKeyContextService = new TestClientKeyContextService(context.ClientKey);
        var tenantClientKeyResolver =
            new TestTenantClientKeyResolver(context.TenantId, context.ClientKey);
        var handler = new FaqItemsSearchFaqItemQueryHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
        var request = new FaqItemsSearchFaqItemQuery
        {
            Request = new FaqItemSearchRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Search = "bluebird",
                FaqIds = [faq.Id]
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Contains("bluebird", result.Items[0].AdditionalInfo, StringComparison.OrdinalIgnoreCase);
    }
}