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
            ShortAnswer = "Use your email.",
            Answer = "Sign in with email and password.",
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
            ShortAnswer = "Updated short",
            Answer = "Updated answer",
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
            ShortAnswer = "Missing",
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
            ShortAnswer = "Short",
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

        var first = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "B question",
            ShortAnswer = "B short",
            Answer = "B answer",
            AdditionalInfo = "B info",
            CtaTitle = "B cta",
            CtaUrl = "https://example.test/b",
            Sort = 1,
            FeedbackScore = 0,
            AiConfidenceScore = 0,
            IsActive = true,
            FaqId = faq.Id,
            TenantId = context.SessionService.TenantId
        };
        var second = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "A question",
            ShortAnswer = "A short",
            Answer = "A answer",
            AdditionalInfo = "A info",
            CtaTitle = "A cta",
            CtaUrl = "https://example.test/a",
            Sort = 5,
            FeedbackScore = 0,
            AiConfidenceScore = 0,
            IsActive = true,
            FaqId = faq.Id,
            TenantId = context.SessionService.TenantId
        };

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

        var first = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "Zulu question",
            ShortAnswer = "Bravo short",
            Answer = "Bravo answer",
            AdditionalInfo = "Bravo info",
            CtaTitle = "Bravo cta",
            CtaUrl = "https://example.test/bravo",
            Sort = 1,
            FeedbackScore = 0,
            AiConfidenceScore = 0,
            IsActive = true,
            FaqId = faq.Id,
            TenantId = context.SessionService.TenantId
        };
        var second = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "Alpha question",
            ShortAnswer = "Alpha short",
            Answer = "Alpha answer",
            AdditionalInfo = "Alpha info",
            CtaTitle = "Alpha cta",
            CtaUrl = "https://example.test/alpha",
            Sort = 1,
            FeedbackScore = 0,
            AiConfidenceScore = 0,
            IsActive = true,
            FaqId = faq.Id,
            TenantId = context.SessionService.TenantId
        };

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

        var first = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "Bravo",
            ShortAnswer = "Short",
            Answer = "Answer",
            AdditionalInfo = "Info",
            CtaTitle = "CTA",
            CtaUrl = "https://example.test/bravo",
            Sort = 1,
            FeedbackScore = 0,
            AiConfidenceScore = 0,
            IsActive = true,
            FaqId = faq.Id,
            TenantId = context.SessionService.TenantId
        };
        var second = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "Alpha",
            ShortAnswer = "Short",
            Answer = "Answer",
            AdditionalInfo = "Info",
            CtaTitle = "CTA",
            CtaUrl = "https://example.test/alpha",
            Sort = 1,
            FeedbackScore = 0,
            AiConfidenceScore = 0,
            IsActive = true,
            FaqId = faq.Id,
            TenantId = context.SessionService.TenantId
        };
        var third = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "Zulu",
            ShortAnswer = "Short",
            Answer = "Answer",
            AdditionalInfo = "Info",
            CtaTitle = "CTA",
            CtaUrl = "https://example.test/zulu",
            Sort = 1,
            FeedbackScore = 0,
            AiConfidenceScore = 0,
            IsActive = false,
            FaqId = faq.Id,
            TenantId = context.SessionService.TenantId
        };

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
            new Common.Persistence.FaqDb.Entities.FaqItem
            {
                Question = "Charlie",
                ShortAnswer = "S",
                Answer = "A",
                AdditionalInfo = "I",
                CtaTitle = "C",
                CtaUrl = "https://example.test/c",
                Sort = 1,
                FeedbackScore = 0,
                AiConfidenceScore = 0,
                IsActive = true,
                FaqId = faq.Id,
                TenantId = context.SessionService.TenantId
            },
            new Common.Persistence.FaqDb.Entities.FaqItem
            {
                Question = "Alpha",
                ShortAnswer = "S",
                Answer = "A",
                AdditionalInfo = "I",
                CtaTitle = "C",
                CtaUrl = "https://example.test/a",
                Sort = 1,
                FeedbackScore = 0,
                AiConfidenceScore = 0,
                IsActive = true,
                FaqId = faq.Id,
                TenantId = context.SessionService.TenantId
            },
            new Common.Persistence.FaqDb.Entities.FaqItem
            {
                Question = "Bravo",
                ShortAnswer = "S",
                Answer = "A",
                AdditionalInfo = "I",
                CtaTitle = "C",
                CtaUrl = "https://example.test/b",
                Sort = 1,
                FeedbackScore = 0,
                AiConfidenceScore = 0,
                IsActive = true,
                FaqId = faq.Id,
                TenantId = context.SessionService.TenantId
            });
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

        var matching = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "How does billing work?",
            ShortAnswer = "Billing short answer",
            Answer = "Billing answer",
            AdditionalInfo = "Billing extra details",
            CtaTitle = "Billing",
            CtaUrl = "https://example.test/billing",
            Sort = 1,
            FeedbackScore = 10,
            AiConfidenceScore = 90,
            IsActive = true,
            FaqId = faqA.Id,
            ContentRefId = contentRefA.Id,
            TenantId = context.SessionService.TenantId
        };
        var nonMatching = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "How does support work?",
            ShortAnswer = "Support short answer",
            Answer = "Support answer",
            AdditionalInfo = "Support extra details",
            CtaTitle = "Support",
            CtaUrl = "https://example.test/support",
            Sort = 2,
            FeedbackScore = 5,
            AiConfidenceScore = 20,
            IsActive = false,
            FaqId = faqB.Id,
            ContentRefId = contentRefB.Id,
            TenantId = context.SessionService.TenantId
        };

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
