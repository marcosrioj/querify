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
               dbContext.Tags.Any() ||
               dbContext.ContentRefs.Any() ||
               dbContext.FaqTags.Any() ||
               dbContext.FaqContentRefs.Any() ||
               dbContext.Votes.Any();
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
                Status = FaqStatus.Published,
                SortStrategy = FaqSortStrategy.Sort,
                CtaEnabled = true,
                CtaTarget = CtaTarget.Blank
            })
            .ToList();

        dbContext.Tags.AddRange(tags);
        dbContext.ContentRefs.AddRange(contentRefs);
        dbContext.Faqs.AddRange(faqs);
        dbContext.SaveChanges();

        var faqItems = new List<FaqItem>();
        var faqTags = new List<FaqTag>();
        var faqContentRefs = new List<FaqContentRef>();
        var votes = new List<Vote>();

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
                var contentRefId = contentRefsByUrl.TryGetValue(itemDefinition.SourceUrl, out var contentRef)
                    ? contentRef.Id
                    : (Guid?)null;

                var voteBatch = BuildVotes(
                    tenantId,
                    itemId,
                    faqIndex,
                    itemIndex,
                    counts.VotesPerItem,
                    itemDefinition.HelpfulVotePercent,
                    unlikeReasons,
                    userAgentSamples);

                faqItems.Add(new FaqItem
                {
                    Id = itemId,
                    TenantId = tenantId,
                    FaqId = faqEntity.Id,
                    Question = itemDefinition.Question,
                    ShortAnswer = itemDefinition.ShortAnswer,
                    Answer = itemDefinition.Answer,
                    AdditionalInfo = $"Official source: {itemDefinition.SourceName}. Retrieved {SourceSnapshotDate}.",
                    CtaTitle = $"Open {itemDefinition.SourceName}",
                    CtaUrl = itemDefinition.SourceUrl,
                    Sort = itemIndex + 1,
                    VoteScore = voteBatch.VoteScore,
                    AiConfidenceScore = Math.Clamp(itemDefinition.AiConfidenceScore, 0, 100),
                    IsActive = true,
                    ContentRefId = contentRefId
                });

                votes.AddRange(voteBatch.Votes);
            }
        }

        dbContext.FaqItems.AddRange(faqItems);
        dbContext.FaqTags.AddRange(faqTags);
        dbContext.FaqContentRefs.AddRange(faqContentRefs);
        dbContext.SaveChanges();

        dbContext.Votes.AddRange(votes);
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

    private static VoteBatch BuildVotes(
        Guid tenantId,
        Guid faqItemId,
        int faqIndex,
        int itemIndex,
        int votesPerItem,
        int helpfulVotePercent,
        UnLikeReason[] unlikeReasons,
        string[] userAgentSamples)
    {
        var safeVotesPerItem = Math.Max(0, votesPerItem);
        if (safeVotesPerItem == 0)
        {
            return new VoteBatch(0, []);
        }

        var likeCount = (int)Math.Round(
            safeVotesPerItem * Math.Clamp(helpfulVotePercent, 0, 100) / 100d,
            MidpointRounding.AwayFromZero);
        likeCount = Math.Clamp(likeCount, 0, safeVotesPerItem);

        var votes = new List<Vote>(safeVotesPerItem);
        for (var voteIndex = 0; voteIndex < safeVotesPerItem; voteIndex++)
        {
            var like = voteIndex < likeCount;
            votes.Add(new Vote
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                FaqItemId = faqItemId,
                Like = like,
                UserPrint = $"seed-feedback-{faqIndex + 1:00}-{itemIndex + 1:00}-{voteIndex + 1:00}",
                Ip = $"198.51.100.{((faqIndex * 40) + (itemIndex * 7) + voteIndex) % 200 + 1}",
                UserAgent = userAgentSamples[(faqIndex + itemIndex + voteIndex) % userAgentSamples.Length],
                UnLikeReason = like ? null : unlikeReasons[(faqIndex + itemIndex + voteIndex) % unlikeReasons.Length]
            });
        }

        return new VoteBatch(
            VoteScore: likeCount - (safeVotesPerItem - likeCount),
            Votes: votes);
    }

    private sealed record VoteBatch(int VoteScore, List<Vote> Votes);
}
