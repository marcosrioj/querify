using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Faq.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Tests.BusinessRules;

public class EntityConstraintsTests
{
    [Fact]
    public async Task Faq_ThrowsWhenNameExceedsMaxLength()
    {
        using var context = TestContext.Create();

        var faq = new Common.Persistence.FaqDb.Entities.Faq
        {
            Name = new string('a', Common.Persistence.FaqDb.Entities.Faq.MaxNameLength + 1),
            Language = "en-US",
            Status = FaqStatus.Draft,
            TenantId = context.SessionService.TenantId
        };

        context.DbContext.Faqs.Add(faq);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Faq_ThrowsWhenLanguageExceedsMaxLength()
    {
        using var context = TestContext.Create();

        var faq = new Common.Persistence.FaqDb.Entities.Faq
        {
            Name = "FAQ",
            Language = new string('b', Common.Persistence.FaqDb.Entities.Faq.MaxLanguageLength + 1),
            Status = FaqStatus.Draft,
            TenantId = context.SessionService.TenantId
        };

        context.DbContext.Faqs.Add(faq);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Tag_ThrowsWhenValueIsMissing()
    {
        using var context = TestContext.Create();

        var tag = new Common.Persistence.FaqDb.Entities.Tag
        {
            Value = null!,
            TenantId = context.SessionService.TenantId
        };

        context.DbContext.Tags.Add(tag);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Tag_ThrowsWhenValueExceedsMaxLength()
    {
        using var context = TestContext.Create();

        var tag = new Common.Persistence.FaqDb.Entities.Tag
        {
            Value = new string('c', Common.Persistence.FaqDb.Entities.Tag.MaxValueLength + 1),
            TenantId = context.SessionService.TenantId
        };

        context.DbContext.Tags.Add(tag);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task ContentRef_ThrowsWhenLocatorIsMissing()
    {
        using var context = TestContext.Create();

        var contentRef = new Common.Persistence.FaqDb.Entities.ContentRef
        {
            Kind = ContentRefKind.Web,
            Locator = null!,
            Label = "Docs",
            Scope = "Scope",
            TenantId = context.SessionService.TenantId
        };

        context.DbContext.ContentRefs.Add(contentRef);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task ContentRef_ThrowsWhenLocatorExceedsMaxLength()
    {
        using var context = TestContext.Create();

        var contentRef = new Common.Persistence.FaqDb.Entities.ContentRef
        {
            Kind = ContentRefKind.Web,
            Locator = new string('d', Common.Persistence.FaqDb.Entities.ContentRef.MaxLocatorLength + 1),
            Label = "Docs",
            Scope = "Scope",
            TenantId = context.SessionService.TenantId
        };

        context.DbContext.ContentRefs.Add(contentRef);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task FaqItem_ThrowsWhenQuestionExceedsMaxLength()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);

        var faqItem = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = new string('e', Common.Persistence.FaqDb.Entities.FaqItem.MaxQuestionLength + 1),
            ShortAnswer = "Short",
            Answer = "Answer",
            AdditionalInfo = "Info",
            CtaTitle = "CTA",
            CtaUrl = "https://example.test/cta",
            Sort = 1,
            VoteScore = 0,
            AiConfidenceScore = 0,
            IsActive = true,
            FaqId = faq.Id,
            TenantId = context.SessionService.TenantId
        };

        context.DbContext.FaqItems.Add(faqItem);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task FaqItem_ThrowsWhenShortAnswerIsMissing()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);

        var faqItem = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "Question",
            ShortAnswer = null!,
            Answer = "Answer",
            AdditionalInfo = "Info",
            CtaTitle = "CTA",
            CtaUrl = "https://example.test/cta",
            Sort = 1,
            VoteScore = 0,
            AiConfidenceScore = 0,
            IsActive = true,
            FaqId = faq.Id,
            TenantId = context.SessionService.TenantId
        };

        context.DbContext.FaqItems.Add(faqItem);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Vote_ThrowsWhenUserAgentExceedsMaxLength()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);
        var faqItem = await TestDataFactory.SeedFaqItemAsync(
            context.DbContext,
            context.SessionService.TenantId,
            faq.Id);

        var vote = new Common.Persistence.FaqDb.Entities.Vote
        {
            Like = true,
            UserPrint = "user",
            Ip = "127.0.0.1",
            UserAgent = new string('f', Common.Persistence.FaqDb.Entities.Vote.MaxUserAgentLength + 1),
            UnLikeReason = null,
            TenantId = context.SessionService.TenantId,
            FaqItemId = faqItem.Id
        };

        context.DbContext.Votes.Add(vote);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.DbContext.SaveChangesAsync());
    }
}