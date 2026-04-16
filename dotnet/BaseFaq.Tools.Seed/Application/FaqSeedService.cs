using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Models.Faq.Enums;
using BaseFaq.Tools.Seed.Abstractions;
using BaseFaq.Tools.Seed.Configuration;
using FaqEntity = BaseFaq.Faq.Common.Persistence.FaqDb.Entities.Faq;

namespace BaseFaq.Tools.Seed.Application;

public sealed class FaqSeedService : IFaqSeedService
{
    private const string SeedLanguage = "en-US";
    private const string SourceSnapshotDate = "2026-04-06";

    public bool HasData(FaqDbContext dbContext)
    {
        return dbContext.Faqs.Any() ||
               dbContext.FaqItems.Any() ||
               dbContext.FaqItemAnswers.Any() ||
               dbContext.Votes.Any() ||
               dbContext.Tags.Any() ||
               dbContext.ContentRefs.Any() ||
               dbContext.FaqTags.Any() ||
               dbContext.FaqContentRefs.Any() ||
               dbContext.Feedbacks.Any();
    }

    public void Seed(FaqDbContext dbContext, Guid tenantId, SeedCounts counts)
    {
        var selectedFaqs = SelectCatalog(counts);
        if (selectedFaqs.Count == 0)
        {
            return;
        }

        var unlikeReasons = Enum.GetValues<UnLikeReason>();
        var userAgentSamples = new[]
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 14_4) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4 Safari/605.1.15",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 18_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.1 Mobile/15E148 Safari/604.1"
        };

        var tags = BuildTags(selectedFaqs, tenantId, counts.TagCount);
        var tagsByValue = tags.ToDictionary(tag => tag.Value, StringComparer.OrdinalIgnoreCase);

        var contentRefs = BuildContentRefs(selectedFaqs, tenantId, counts.ContentRefCount);
        var contentRefsByUrl = contentRefs.ToDictionary(contentRef => contentRef.Locator, StringComparer.OrdinalIgnoreCase);

        var faqs = selectedFaqs
            .Select(definition => new FaqEntity
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = definition.Name,
                Language = SeedLanguage,
                Status = FaqStatus.Published
            })
            .ToList();

        dbContext.Tags.AddRange(tags);
        dbContext.ContentRefs.AddRange(contentRefs);
        dbContext.Faqs.AddRange(faqs);
        dbContext.SaveChanges();

        var faqItems = new List<FaqItem>();
        var faqItemAnswers = new List<FaqItemAnswer>();
        var faqTags = new List<FaqTag>();
        var faqContentRefs = new List<FaqContentRef>();
        var feedbacks = new List<Feedback>();

        for (var faqIndex = 0; faqIndex < selectedFaqs.Count; faqIndex++)
        {
            var faqDefinition = selectedFaqs[faqIndex];
            var faqEntity = faqs[faqIndex];

            foreach (var tagValue in faqDefinition.Tags
                         .Where(tagsByValue.ContainsKey)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .Take(Math.Max(0, counts.TagsPerFaq)))
            {
                faqTags.Add(new FaqTag
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    FaqId = faqEntity.Id,
                    TagId = tagsByValue[tagValue].Id
                });
            }

            foreach (var sourceUrl in faqDefinition.Items
                         .Select(item => item.SourceUrl)
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .Where(contentRefsByUrl.ContainsKey)
                         .Take(Math.Max(0, counts.ContentRefsPerFaq)))
            {
                faqContentRefs.Add(new FaqContentRef
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    FaqId = faqEntity.Id,
                    ContentRefId = contentRefsByUrl[sourceUrl].Id
                });
            }

            for (var itemIndex = 0; itemIndex < faqDefinition.Items.Count; itemIndex++)
            {
                var itemDefinition = faqDefinition.Items[itemIndex];
                var itemId = Guid.NewGuid();
                var answerId = Guid.NewGuid();
                var contentRefId = contentRefsByUrl.TryGetValue(itemDefinition.SourceUrl, out var contentRef)
                    ? contentRef.Id
                    : (Guid?)null;

                var feedbackBatch = BuildFeedbacks(
                    tenantId,
                    itemId,
                    faqIndex,
                    itemIndex,
                    counts.FeedbacksPerItem,
                    itemDefinition.HelpfulFeedbackPercent,
                    unlikeReasons,
                    userAgentSamples);

                faqItems.Add(new FaqItem
                {
                    Id = itemId,
                    TenantId = tenantId,
                    FaqId = faqEntity.Id,
                    Question = itemDefinition.Question,
                    AdditionalInfo = $"Official source: {itemDefinition.SourceName}. Retrieved {SourceSnapshotDate}.",
                    CtaTitle = $"Open {itemDefinition.SourceName}",
                    CtaUrl = itemDefinition.SourceUrl,
                    Sort = itemIndex + 1,
                    FeedbackScore = feedbackBatch.FeedbackScore,
                    ConfidenceScore = Math.Clamp(itemDefinition.ConfidenceScore, 0, 100),
                    IsActive = true,
                    ContentRefId = contentRefId
                });

                faqItemAnswers.Add(new FaqItemAnswer
                {
                    Id = answerId,
                    TenantId = tenantId,
                    FaqItemId = itemId,
                    ShortAnswer = itemDefinition.ShortAnswer,
                    Answer = itemDefinition.Answer,
                    Sort = 1,
                    VoteScore = 0,
                    IsActive = true
                });

                feedbacks.AddRange(feedbackBatch.Feedbacks);
            }
        }

        dbContext.FaqItems.AddRange(faqItems);
        dbContext.FaqItemAnswers.AddRange(faqItemAnswers);
        dbContext.FaqTags.AddRange(faqTags);
        dbContext.FaqContentRefs.AddRange(faqContentRefs);
        dbContext.SaveChanges();

        dbContext.Feedbacks.AddRange(feedbacks);
        dbContext.SaveChanges();
    }

    private static List<SeedFaqDefinition> SelectCatalog(SeedCounts counts)
    {
        return FaqSeedCatalog.Build()
            .Take(Math.Max(0, counts.FaqCount))
            .Select(definition => definition with
            {
                Items = definition.Items.Take(Math.Max(0, counts.ItemsPerFaq)).ToArray()
            })
            .Where(definition => definition.Items.Count > 0)
            .ToList();
    }

    private static List<Tag> BuildTags(
        IReadOnlyCollection<SeedFaqDefinition> faqDefinitions,
        Guid tenantId,
        int maxTagCount)
    {
        return faqDefinitions
            .SelectMany(definition => definition.Tags)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(0, maxTagCount))
            .Select(tagValue => new Tag
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Value = tagValue
            })
            .ToList();
    }

    private static List<ContentRef> BuildContentRefs(
        IReadOnlyCollection<SeedFaqDefinition> faqDefinitions,
        Guid tenantId,
        int maxContentRefCount)
    {
        return faqDefinitions
            .SelectMany(definition => definition.Items)
            .DistinctBy(item => item.SourceUrl, StringComparer.OrdinalIgnoreCase)
            .Take(Math.Max(0, maxContentRefCount))
            .Select(item => new ContentRef
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Kind = ContentRefKind.Web,
                Locator = item.SourceUrl,
                Label = item.SourceLabel
            })
            .ToList();
    }

    private static FeedbackBatch BuildFeedbacks(
        Guid tenantId,
        Guid faqItemId,
        int faqIndex,
        int itemIndex,
        int feedbacksPerItem,
        int helpfulFeedbackPercent,
        UnLikeReason[] unlikeReasons,
        string[] userAgentSamples)
    {
        var safeFeedbacksPerItem = Math.Max(0, feedbacksPerItem);
        if (safeFeedbacksPerItem == 0)
        {
            return new FeedbackBatch(0, []);
        }

        var likeCount = (int)Math.Round(
            safeFeedbacksPerItem * Math.Clamp(helpfulFeedbackPercent, 0, 100) / 100d,
            MidpointRounding.AwayFromZero);
        likeCount = Math.Clamp(likeCount, 0, safeFeedbacksPerItem);

        var feedbacks = new List<Feedback>(safeFeedbacksPerItem);
        for (var feedbackIndex = 0; feedbackIndex < safeFeedbacksPerItem; feedbackIndex++)
        {
            var like = feedbackIndex < likeCount;
            feedbacks.Add(new Feedback
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                FaqItemId = faqItemId,
                Like = like,
                UserPrint = $"seed-feedback-{faqIndex + 1:00}-{itemIndex + 1:00}-{feedbackIndex + 1:00}",
                Ip = $"198.51.100.{((faqIndex * 40) + (itemIndex * 7) + feedbackIndex) % 200 + 1}",
                UserAgent = userAgentSamples[(faqIndex + itemIndex + feedbackIndex) % userAgentSamples.Length],
                UnLikeReason = like ? null : unlikeReasons[(faqIndex + itemIndex + feedbackIndex) % unlikeReasons.Length]
            });
        }

        return new FeedbackBatch(
            FeedbackScore: likeCount - (safeFeedbacksPerItem - likeCount),
            Feedbacks: feedbacks);
    }

    private sealed record FeedbackBatch(int FeedbackScore, List<Feedback> Feedbacks);
}
