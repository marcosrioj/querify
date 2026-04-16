using BaseFaq.Faq.Public.Business.FaqItem.Commands.CreateFaqItem;
using BaseFaq.Faq.Public.Business.FaqItem.Queries.SearchFaqItem;
using BaseFaq.Faq.Public.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Faq.Dtos.FaqItem;
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
        var handler = new FaqItemsCreateFaqItemCommandHandler(
            context.DbContext,
            clientKeyContextService,
            tenantClientKeyResolver,
            context.HttpContextAccessor);
        var request = new FaqItemsCreateFaqItemCommand
        {
            Question = "How to sign in?",
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
    public async Task SearchFaqItems_OrdersByDefaultSort()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.TenantId);

        var first = TestDataFactory.CreateFaqItem(
            context.TenantId,
            faq.Id,
            question: "First sort",
            shortAnswer: "First",
            answer: "First",
            additionalInfo: "First",
            ctaTitle: "First",
            ctaUrl: "https://example.test/first",
            sort: 1,
            feedbackScore: 5,
            confidenceScore: 10);
        var second = TestDataFactory.CreateFaqItem(
            context.TenantId,
            faq.Id,
            question: "Second sort",
            shortAnswer: "Second",
            answer: "Second",
            additionalInfo: "Second",
            ctaTitle: "Second",
            ctaUrl: "https://example.test/second",
            sort: 2,
            feedbackScore: 20,
            confidenceScore: 50);

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
            TestDataFactory.CreateFaqItem(
                context.TenantId,
                faq.Id,
                question: "How to reset MFA?",
                shortAnswer: "Use security page",
                answer: "Go to security settings",
                additionalInfo: "Contact support if locked out",
                ctaTitle: "Security",
                ctaUrl: "https://example.test/security",
                sort: 1,
                feedbackScore: 0,
                confidenceScore: 0),
            TestDataFactory.CreateFaqItem(
                context.TenantId,
                faq.Id,
                question: "How to change billing address?",
                shortAnswer: "Use billing page",
                answer: "Open billing settings",
                additionalInfo: "Requires owner role",
                ctaTitle: "Billing",
                ctaUrl: "https://example.test/billing",
                sort: 2,
                feedbackScore: 0,
                confidenceScore: 0));
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
            TestDataFactory.CreateFaqItem(
                context.TenantId,
                faq.Id,
                question: "Q1",
                shortAnswer: "A1",
                answer: "A1",
                additionalInfo: "I1",
                ctaTitle: "C1",
                ctaUrl: "https://example.test/1",
                sort: 1,
                feedbackScore: 0,
                confidenceScore: 0),
            TestDataFactory.CreateFaqItem(
                context.TenantId,
                faq.Id,
                question: "Q2",
                shortAnswer: "A2",
                answer: "A2",
                additionalInfo: "I2",
                ctaTitle: "C2",
                ctaUrl: "https://example.test/2",
                sort: 2,
                feedbackScore: 0,
                confidenceScore: 0),
            TestDataFactory.CreateFaqItem(
                context.TenantId,
                faq.Id,
                question: "Q3",
                shortAnswer: "A3",
                answer: "A3",
                additionalInfo: "I3",
                ctaTitle: "C3",
                ctaUrl: "https://example.test/3",
                sort: 3,
                feedbackScore: 0,
                confidenceScore: 0));
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
            TestDataFactory.CreateFaqItem(
                context.TenantId,
                faq.Id,
                question: "General question",
                shortAnswer: "General short",
                answer: "General answer",
                additionalInfo: "Contains custom token: bluebird",
                ctaTitle: "General",
                ctaUrl: "https://example.test/general",
                sort: 1,
                feedbackScore: 0,
                confidenceScore: 0),
            TestDataFactory.CreateFaqItem(
                context.TenantId,
                faq.Id,
                question: "Another question",
                shortAnswer: "Another short",
                answer: "Another answer",
                additionalInfo: "No token here",
                ctaTitle: "Another",
                ctaUrl: "https://example.test/another",
                sort: 2,
                feedbackScore: 0,
                confidenceScore: 0));
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
