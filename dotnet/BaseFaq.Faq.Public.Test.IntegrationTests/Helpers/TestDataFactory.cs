using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Models.Faq.Enums;

namespace BaseFaq.Faq.Public.Test.IntegrationTests.Helpers;

public static class TestDataFactory
{
    public static async Task<Common.Persistence.FaqDb.Entities.Faq> SeedFaqAsync(
        FaqDbContext dbContext,
        Guid tenantId,
        string? name = null,
        string? language = null,
        FaqStatus status = FaqStatus.Draft)
    {
        var faq = new Common.Persistence.FaqDb.Entities.Faq
        {
            Name = name ?? "General FAQ",
            Language = language ?? "en-US",
            Status = status,
            TenantId = tenantId
        };

        dbContext.Faqs.Add(faq);
        await dbContext.SaveChangesAsync();
        return faq;
    }

    public static async Task<Tag> SeedTagAsync(
        FaqDbContext dbContext,
        Guid tenantId,
        string? value = null)
    {
        var tag = new Tag
        {
            Value = value ?? "shipping",
            TenantId = tenantId
        };

        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync();
        return tag;
    }

    public static async Task<ContentRef> SeedContentRefAsync(
        FaqDbContext dbContext,
        Guid tenantId,
        ContentRefKind kind = ContentRefKind.Web,
        string? locator = null)
    {
        var contentRef = new ContentRef
        {
            Kind = kind,
            Locator = locator ?? "https://www.example.com/docs",
            Label = "Docs",
            Scope = "Section 1",
            TenantId = tenantId
        };

        dbContext.ContentRefs.Add(contentRef);
        await dbContext.SaveChangesAsync();
        return contentRef;
    }

    public static async Task<FaqItem> SeedFaqItemAsync(
        FaqDbContext dbContext,
        Guid tenantId,
        Guid faqId,
        Guid? contentRefId = null)
    {
        var faqItem = new FaqItem
        {
            Question = "How do I reset my password?",
            ShortAnswer = "Use the reset link.",
            Answer = "Click reset in settings.",
            AdditionalInfo = "Support can help if needed.",
            CtaTitle = "Reset",
            CtaUrl = "https://example.test/reset",
            Sort = 1,
            FeedbackScore = 10,
            AiConfidenceScore = 80,
            IsActive = true,
            FaqId = faqId,
            ContentRefId = contentRefId,
            TenantId = tenantId
        };

        dbContext.FaqItems.Add(faqItem);
        await dbContext.SaveChangesAsync();
        return faqItem;
    }

    public static async Task<Feedback> SeedFeedbackAsync(
        FaqDbContext dbContext,
        Guid tenantId,
        Guid faqItemId,
        bool like = true)
    {
        var feedback = new Feedback
        {
            Like = like,
            UserPrint = "user-print",
            Ip = "127.0.0.1",
            UserAgent = "TestAgent",
            UnLikeReason = like ? null : UnLikeReason.NotRelevant,
            TenantId = tenantId,
            FaqItemId = faqItemId
        };

        dbContext.Feedbacks.Add(feedback);
        await dbContext.SaveChangesAsync();
        return feedback;
    }

    public static async Task<FaqTag> SeedFaqTagAsync(
        FaqDbContext dbContext,
        Guid tenantId,
        Guid faqId,
        Guid tagId)
    {
        var faqTag = new FaqTag
        {
            FaqId = faqId,
            TagId = tagId,
            TenantId = tenantId
        };

        dbContext.FaqTags.Add(faqTag);
        await dbContext.SaveChangesAsync();
        return faqTag;
    }

    public static async Task<FaqContentRef> SeedFaqContentRefAsync(
        FaqDbContext dbContext,
        Guid tenantId,
        Guid faqId,
        Guid contentRefId)
    {
        var faqContentRef = new FaqContentRef
        {
            FaqId = faqId,
            ContentRefId = contentRefId,
            TenantId = tenantId
        };

        dbContext.FaqContentRefs.Add(faqContentRef);
        await dbContext.SaveChangesAsync();
        return faqContentRef;
    }
}
