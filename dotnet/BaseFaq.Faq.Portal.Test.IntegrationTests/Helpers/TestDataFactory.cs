using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Models.Faq.Enums;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;

public static class TestDataFactory
{
    public static FaqItem CreateFaqItem(
        Guid tenantId,
        Guid faqId,
        Guid? contentRefId = null,
        string? question = null,
        string? shortAnswer = null,
        string? answer = null,
        string? additionalInfo = null,
        string? ctaTitle = null,
        string? ctaUrl = null,
        int sort = 1,
        int feedbackScore = 10,
        int confidenceScore = 80,
        bool isActive = true,
        int answerSort = 1,
        int voteScore = 0,
        bool answerIsActive = true)
    {
        var faqItemId = Guid.NewGuid();
        var faqItem = new FaqItem
        {
            Id = faqItemId,
            Question = question ?? "How do I reset my password?",
            AdditionalInfo = additionalInfo ?? "Support can help if needed.",
            CtaTitle = ctaTitle ?? "Reset",
            CtaUrl = ctaUrl ?? "https://example.test/reset",
            Sort = sort,
            FeedbackScore = feedbackScore,
            ConfidenceScore = confidenceScore,
            IsActive = isActive,
            FaqId = faqId,
            ContentRefId = contentRefId,
            TenantId = tenantId
        };

        faqItem.Answers.Add(new FaqItemAnswer
        {
            ShortAnswer = shortAnswer ?? "Use the reset link.",
            Answer = answer ?? "Click reset in settings.",
            Sort = answerSort,
            VoteScore = voteScore,
            IsActive = answerIsActive,
            FaqItemId = faqItemId,
            TenantId = tenantId
        });

        return faqItem;
    }

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
        Guid? contentRefId = null,
        string? question = null,
        string? shortAnswer = null,
        string? answer = null,
        string? additionalInfo = null,
        string? ctaTitle = null,
        string? ctaUrl = null,
        int sort = 1,
        int feedbackScore = 10,
        int confidenceScore = 80,
        bool isActive = true)
    {
        var faqItem = CreateFaqItem(
            tenantId,
            faqId,
            contentRefId,
            question,
            shortAnswer,
            answer,
            additionalInfo,
            ctaTitle,
            ctaUrl,
            sort,
            feedbackScore,
            confidenceScore,
            isActive);

        dbContext.FaqItems.Add(faqItem);
        await dbContext.SaveChangesAsync();
        return faqItem;
    }

    public static async Task<FaqItemAnswer> SeedFaqItemAnswerAsync(
        FaqDbContext dbContext,
        Guid tenantId,
        Guid faqItemId,
        string? shortAnswer = null,
        string? answer = null,
        int sort = 1,
        int voteScore = 0,
        bool isActive = true)
    {
        var faqItemAnswer = new FaqItemAnswer
        {
            ShortAnswer = shortAnswer ?? "Use the reset link.",
            Answer = answer ?? "Click reset in settings.",
            Sort = sort,
            VoteScore = voteScore,
            IsActive = isActive,
            TenantId = tenantId,
            FaqItemId = faqItemId
        };

        dbContext.FaqItemAnswers.Add(faqItemAnswer);
        await dbContext.SaveChangesAsync();
        return faqItemAnswer;
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

    public static async Task<Vote> SeedVoteAsync(
        FaqDbContext dbContext,
        Guid tenantId,
        Guid faqItemAnswerId,
        string? userPrint = null,
        string? ip = null,
        string? userAgent = null)
    {
        var vote = new Vote
        {
            UserPrint = userPrint ?? "user-print",
            Ip = ip ?? "127.0.0.1",
            UserAgent = userAgent ?? "TestAgent",
            TenantId = tenantId,
            FaqItemAnswerId = faqItemAnswerId
        };

        dbContext.Votes.Add(vote);
        await dbContext.SaveChangesAsync();
        return vote;
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
