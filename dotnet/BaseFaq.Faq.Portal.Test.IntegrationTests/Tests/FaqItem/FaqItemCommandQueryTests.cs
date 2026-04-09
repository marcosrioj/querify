using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Portal.Business.FaqItem.Commands.CreateFaqItem;
using BaseFaq.Faq.Portal.Business.FaqItem.Commands.DeleteFaqItem;
using BaseFaq.Faq.Portal.Business.FaqItem.Commands.UpdateFaqItem;
using BaseFaq.Faq.Portal.Business.FaqItem.Queries.GetFaqItem;
using BaseFaq.Faq.Portal.Business.FaqItem.Queries.GetFaqItemList;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Faq.Dtos.FaqItem;
using BaseFaq.Models.Faq.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Tests.FaqItem;

public class FaqItemCommandQueryTests
{
    [Fact]
    public async Task CreateFaqItem_PersistsEntityAndReturnsId()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);

        var handler = new FaqItemsCreateFaqItemCommandHandler(context.DbContext, context.SessionService);
        var request = new FaqItemsCreateFaqItemCommand
        {
            Question = "How to sign in?",
            AdditionalInfo = "Contact support if needed.",
            CtaTitle = "Sign in",
            CtaUrl = "https://example.test/login",
            Sort = 2,
            IsActive = true,
            FaqId = faq.Id,
            ContentRefId = contentRef.Id
        };

        var id = await handler.Handle(request, CancellationToken.None);

        var faqItem = await context.DbContext.FaqItems.FindAsync(id);
        Assert.NotNull(faqItem);
        Assert.Equal("How to sign in?", faqItem!.Question);
        Assert.Equal(faq.Id, faqItem.FaqId);
        Assert.Equal(contentRef.Id, faqItem.ContentRefId);
        Assert.Equal(context.SessionService.TenantId, faqItem.TenantId);
    }

    [Fact]
    public async Task UpdateFaqItem_UpdatesExistingFaqItem()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var otherFaq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "Other");
        var contentRef = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id,
            contentRef.Id);

        var handler = new FaqItemsUpdateFaqItemCommandHandler(context.DbContext);
        var request = new FaqItemsUpdateFaqItemCommand
        {
            Id = faqItem.Id,
            Question = "Updated question",
            AdditionalInfo = "Updated info",
            CtaTitle = "Updated CTA",
            CtaUrl = "https://example.test/updated",
            Sort = 5,
            IsActive = false,
            FaqId = otherFaq.Id,
            ContentRefId = null
        };

        await handler.Handle(request, CancellationToken.None);

        var updated = await context.DbContext.FaqItems.FindAsync(faqItem.Id);
        Assert.NotNull(updated);
        Assert.Equal("Updated question", updated!.Question);
        Assert.Equal(otherFaq.Id, updated.FaqId);
        Assert.Null(updated.ContentRefId);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task UpdateFaqItem_ThrowsWhenMissing()
    {
        using var context = TestContext.Create();
        var handler = new FaqItemsUpdateFaqItemCommandHandler(context.DbContext);
        var request = new FaqItemsUpdateFaqItemCommand
        {
            Id = Guid.NewGuid(),
            Question = "Missing",
            Sort = 0,
            IsActive = false,
            FaqId = Guid.NewGuid()
        };

        var exception =
            await Assert.ThrowsAsync<ApiErrorException>(() => handler.Handle(request, CancellationToken.None));

        Assert.Equal(404, exception.ErrorCode);
    }

    [Fact]
    public async Task DeleteFaqItem_SoftDeletesEntity()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);

        var handler = new FaqItemsDeleteFaqItemCommandHandler(context.DbContext);
        await handler.Handle(new FaqItemsDeleteFaqItemCommand { Id = faqItem.Id }, CancellationToken.None);

        context.DbContext.SoftDeleteFiltersEnabled = false;
        var deleted = await context.DbContext.FaqItems.FindAsync(faqItem.Id);
        Assert.NotNull(deleted);
        Assert.True(deleted!.IsDeleted);
    }

    [Fact]
    public async Task CreateFaqItem_ThrowsWhenFaqMissing()
    {
        using var context = TestContext.Create();

        var handler = new FaqItemsCreateFaqItemCommandHandler(context.DbContext, context.SessionService);
        var request = new FaqItemsCreateFaqItemCommand
        {
            Question = "Question",
            Sort = 1,
            IsActive = true,
            FaqId = Guid.NewGuid(),
            ContentRefId = null
        };

        await Assert.ThrowsAsync<DbUpdateException>(() => handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetFaqItem_ReturnsDto()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);

        var handler = new FaqItemsGetFaqItemQueryHandler(context.DbContext);
        var result = await handler.Handle(new FaqItemsGetFaqItemQuery { Id = faqItem.Id }, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(faqItem.Id, result!.Id);
        Assert.Equal(faqItem.Question, result.Question);
        Assert.Equal(faqItem.FaqId, result.FaqId);
        Assert.Equal(faqItem.ContentRefId, result.ContentRefId);
    }

    [Fact]
    public async Task GetFaqItemList_ReturnsPagedItems()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.SessionService.TenantId, faq.Id);
        await TestDataFactory.SeedFaqItemAsync(context.DbContext, context.SessionService.TenantId, faq.Id);

        var handler = new FaqItemsGetFaqItemListQueryHandler(context.DbContext);
        var request = new FaqItemsGetFaqItemListQuery
        {
            Request = new FaqItemGetAllRequestDto { SkipCount = 0, MaxResultCount = 10 }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetFaqItemList_SortsByExplicitField()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);

        var first = TestDataFactory.CreateFaqItem(
            context.SessionService.TenantId,
            faq.Id,
            question: "B question",
            shortAnswer: "B short",
            answer: "B answer",
            additionalInfo: "B info",
            ctaTitle: "B cta",
            ctaUrl: "https://example.test/b",
            sort: 1,
            feedbackScore: 0,
            aiConfidenceScore: 0);
        var second = TestDataFactory.CreateFaqItem(
            context.SessionService.TenantId,
            faq.Id,
            question: "A question",
            shortAnswer: "A short",
            answer: "A answer",
            additionalInfo: "A info",
            ctaTitle: "A cta",
            ctaUrl: "https://example.test/a",
            sort: 5,
            feedbackScore: 0,
            aiConfidenceScore: 0);

        context.DbContext.FaqItems.AddRange(first, second);
        await context.DbContext.SaveChangesAsync();

        var handler = new FaqItemsGetFaqItemListQueryHandler(context.DbContext);
        var request = new FaqItemsGetFaqItemListQuery
        {
            Request = new FaqItemGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "sort DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(second.Id, result.Items[0].Id);
        Assert.Equal(first.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task GetFaqItemList_FallsBackToUpdatedDateWhenSortingInvalid()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);

        var first = TestDataFactory.CreateFaqItem(
            context.SessionService.TenantId,
            faq.Id,
            question: "Zulu question",
            shortAnswer: "Bravo short",
            answer: "Bravo answer",
            additionalInfo: "Bravo info",
            ctaTitle: "Bravo cta",
            ctaUrl: "https://example.test/bravo",
            sort: 1,
            feedbackScore: 0,
            aiConfidenceScore: 0);
        var second = TestDataFactory.CreateFaqItem(
            context.SessionService.TenantId,
            faq.Id,
            question: "Alpha question",
            shortAnswer: "Alpha short",
            answer: "Alpha answer",
            additionalInfo: "Alpha info",
            ctaTitle: "Alpha cta",
            ctaUrl: "https://example.test/alpha",
            sort: 1,
            feedbackScore: 0,
            aiConfidenceScore: 0);

        context.DbContext.FaqItems.AddRange(first, second);
        await context.DbContext.SaveChangesAsync();
        first.IsActive = false;
        await context.DbContext.SaveChangesAsync();

        var handler = new FaqItemsGetFaqItemListQueryHandler(context.DbContext);
        var request = new FaqItemsGetFaqItemListQuery
        {
            Request = new FaqItemGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "unknown DESC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(first.Id, result.Items[0].Id);
        Assert.Equal(second.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task GetFaqItemList_SortsByMultipleFields()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);

        var first = TestDataFactory.CreateFaqItem(
            context.SessionService.TenantId,
            faq.Id,
            question: "Bravo",
            shortAnswer: "Short",
            answer: "Answer",
            additionalInfo: "Info",
            ctaTitle: "CTA",
            ctaUrl: "https://example.test/bravo",
            sort: 1,
            feedbackScore: 0,
            aiConfidenceScore: 0);
        var second = TestDataFactory.CreateFaqItem(
            context.SessionService.TenantId,
            faq.Id,
            question: "Alpha",
            shortAnswer: "Short",
            answer: "Answer",
            additionalInfo: "Info",
            ctaTitle: "CTA",
            ctaUrl: "https://example.test/alpha",
            sort: 1,
            feedbackScore: 0,
            aiConfidenceScore: 0);
        var third = TestDataFactory.CreateFaqItem(
            context.SessionService.TenantId,
            faq.Id,
            question: "Zulu",
            shortAnswer: "Short",
            answer: "Answer",
            additionalInfo: "Info",
            ctaTitle: "CTA",
            ctaUrl: "https://example.test/zulu",
            sort: 1,
            feedbackScore: 0,
            aiConfidenceScore: 0,
            isActive: false);

        context.DbContext.FaqItems.AddRange(first, second, third);
        await context.DbContext.SaveChangesAsync();

        var handler = new FaqItemsGetFaqItemListQueryHandler(context.DbContext);
        var request = new FaqItemsGetFaqItemListQuery
        {
            Request = new FaqItemGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                Sorting = "isactive DESC, question ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(second.Id, result.Items[0].Id);
        Assert.Equal(first.Id, result.Items[1].Id);
        Assert.Equal(third.Id, result.Items[2].Id);
    }

    [Fact]
    public async Task GetFaqItemList_AppliesPaginationWindow()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);

        context.DbContext.FaqItems.AddRange(
            TestDataFactory.CreateFaqItem(
                context.SessionService.TenantId,
                faq.Id,
                question: "Charlie",
                shortAnswer: "S",
                answer: "A",
                additionalInfo: "I",
                ctaTitle: "C",
                ctaUrl: "https://example.test/c",
                sort: 1,
                feedbackScore: 0,
                aiConfidenceScore: 0),
            TestDataFactory.CreateFaqItem(
                context.SessionService.TenantId,
                faq.Id,
                question: "Alpha",
                shortAnswer: "S",
                answer: "A",
                additionalInfo: "I",
                ctaTitle: "C",
                ctaUrl: "https://example.test/a",
                sort: 1,
                feedbackScore: 0,
                aiConfidenceScore: 0),
            TestDataFactory.CreateFaqItem(
                context.SessionService.TenantId,
                faq.Id,
                question: "Bravo",
                shortAnswer: "S",
                answer: "A",
                additionalInfo: "I",
                ctaTitle: "C",
                ctaUrl: "https://example.test/b",
                sort: 1,
                feedbackScore: 0,
                aiConfidenceScore: 0));
        await context.DbContext.SaveChangesAsync();

        var handler = new FaqItemsGetFaqItemListQueryHandler(context.DbContext);
        var request = new FaqItemsGetFaqItemListQuery
        {
            Request = new FaqItemGetAllRequestDto
            {
                SkipCount = 1,
                MaxResultCount = 1,
                Sorting = "question ASC"
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Bravo", result.Items[0].Question);
    }

    [Fact]
    public async Task GetFaqItemList_FiltersByFaqContentRefActiveStateAndSearchText()
    {
        using var context = TestContext.Create();
        var faqA = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "FAQ A");
        var faqB = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId, "FAQ B");
        var contentRefA = await TestDataFactory.SeedContentRefAsync(context.DbContext, context.SessionService.TenantId);
        var contentRefB = await TestDataFactory.SeedContentRefAsync(
            context.DbContext,
            context.SessionService.TenantId,
            ContentRefKind.Video,
            "https://www.example.com/video");

        var matching = TestDataFactory.CreateFaqItem(
            context.SessionService.TenantId,
            faqA.Id,
            contentRefA.Id,
            question: "How does billing work?",
            shortAnswer: "Billing short answer",
            answer: "Billing answer",
            additionalInfo: "Billing extra details",
            ctaTitle: "Billing",
            ctaUrl: "https://example.test/billing",
            sort: 1,
            feedbackScore: 10,
            aiConfidenceScore: 90,
            isActive: true);
        var nonMatching = TestDataFactory.CreateFaqItem(
            context.SessionService.TenantId,
            faqB.Id,
            contentRefB.Id,
            question: "How does support work?",
            shortAnswer: "Support short answer",
            answer: "Support answer",
            additionalInfo: "Support extra details",
            ctaTitle: "Support",
            ctaUrl: "https://example.test/support",
            sort: 2,
            feedbackScore: 5,
            aiConfidenceScore: 20,
            isActive: false);

        context.DbContext.FaqItems.AddRange(matching, nonMatching);
        await context.DbContext.SaveChangesAsync();

        var handler = new FaqItemsGetFaqItemListQueryHandler(context.DbContext);
        var request = new FaqItemsGetFaqItemListQuery
        {
            Request = new FaqItemGetAllRequestDto
            {
                SkipCount = 0,
                MaxResultCount = 10,
                SearchText = "billing",
                FaqId = faqA.Id,
                ContentRefId = contentRefA.Id,
                IsActive = true
            }
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(matching.Id, result.Items[0].Id);
    }
}
