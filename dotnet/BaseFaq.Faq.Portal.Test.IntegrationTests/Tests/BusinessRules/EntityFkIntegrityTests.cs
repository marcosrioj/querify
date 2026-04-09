using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Faq.Portal.Test.IntegrationTests.Helpers;
using BaseFaq.Models.Faq.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BaseFaq.Faq.Portal.Test.IntegrationTests.Tests.BusinessRules;

public class EntityFkIntegrityTests
{
    [Fact]
    public async Task FaqItem_ThrowsWhenFaqDoesNotExist()
    {
        using var context = TestContext.Create();

        var faqItem = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "Question",
            AdditionalInfo = "Info",
            CtaTitle = "CTA",
            CtaUrl = "https://example.test/cta",
            Sort = 1,
            FeedbackScore = 0,
            AiConfidenceScore = 0,
            IsActive = true,
            FaqId = Guid.NewGuid(),
            TenantId = context.SessionService.TenantId
        };

        context.DbContext.FaqItems.Add(faqItem);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task FaqItem_ThrowsWhenContentRefDoesNotExist()
    {
        using var context = TestContext.Create();
        var faq = await TestDataFactory.SeedFaqAsync(context.DbContext, context.SessionService.TenantId);

        var faqItem = new Common.Persistence.FaqDb.Entities.FaqItem
        {
            Question = "Question",
            AdditionalInfo = "Info",
            CtaTitle = "CTA",
            CtaUrl = "https://example.test/cta",
            Sort = 1,
            FeedbackScore = 0,
            AiConfidenceScore = 0,
            IsActive = true,
            FaqId = faq.Id,
            ContentRefId = Guid.NewGuid(),
            TenantId = context.SessionService.TenantId
        };

        context.DbContext.FaqItems.Add(faqItem);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Feedback_ThrowsWhenFaqItemDoesNotExist()
    {
        using var context = TestContext.Create();

        var feedback = new Common.Persistence.FaqDb.Entities.Feedback
        {
            Like = true,
            UserPrint = "user",
            Ip = "127.0.0.1",
            UserAgent = "agent",
            UnLikeReason = null,
            TenantId = context.SessionService.TenantId,
            FaqItemId = Guid.NewGuid()
        };

        context.DbContext.Feedbacks.Add(feedback);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.DbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task Vote_ThrowsWhenFaqItemAnswerDoesNotExist()
    {
        using var context = TestContext.Create();

        var vote = new Common.Persistence.FaqDb.Entities.Vote
        {
            UserPrint = "user",
            Ip = "127.0.0.1",
            UserAgent = "agent",
            TenantId = context.SessionService.TenantId,
            FaqItemAnswerId = Guid.NewGuid()
        };

        context.DbContext.Votes.Add(vote);

        await Assert.ThrowsAsync<DbUpdateException>(() => context.DbContext.SaveChangesAsync());
    }
}
