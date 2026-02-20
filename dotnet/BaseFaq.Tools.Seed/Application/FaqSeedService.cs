using BaseFaq.Faq.Common.Persistence.FaqDb;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using BaseFaq.Tools.Seed.Abstractions;
using BaseFaq.Tools.Seed.Configuration;
using BaseFaq.Models.Faq.Enums;

namespace BaseFaq.Tools.Seed.Application;

public sealed class FaqSeedService : IFaqSeedService
{
    private const int SeedRandom = 2048;

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
        var random = new Random(SeedRandom);
        var statuses = Enum.GetValues<FaqStatus>();
        var sortStrategies = Enum.GetValues<FaqSortStrategy>();
        var contentKinds = Enum.GetValues<ContentRefKind>();
        var unlikeReasons = Enum.GetValues<UnLikeReason>();

        var tags = BuildTags(counts.TagCount, tenantId);
        var contentRefs = BuildContentRefs(counts.ContentRefCount, tenantId, contentKinds, random);
        var faqs = BuildFaqs(counts.FaqCount, tenantId, statuses, sortStrategies);

        dbContext.Tags.AddRange(tags);
        dbContext.ContentRefs.AddRange(contentRefs);
        dbContext.Faqs.AddRange(faqs);
        dbContext.SaveChanges();

        var faqItems = new List<FaqItem>();
        var faqTags = new List<FaqTag>();
        var faqContentRefs = new List<FaqContentRef>();
        var votes = new List<Vote>();

        var tagCursor = 0;
        var contentCursor = 0;
        var userAgentSamples = new[]
        {
            "Mozilla/5.0 (Seed) BaseFaqBot/1.0",
            "Mozilla/5.0 (Seed) BaseFaqBot/1.1",
            "Mozilla/5.0 (Seed) BaseFaqBot/1.2"
        };

        foreach (var faq in faqs)
        {
            for (var i = 1; i <= counts.ItemsPerFaq; i++)
            {
                var itemId = Guid.NewGuid();
                var includeAnswer = i % 5 != 0;
                var includeAdditional = i % 4 == 0;
                var includeCta = faq.CtaEnabled && i % 3 == 0;
                var contentRefId = i % 2 == 0 ? contentRefs[contentCursor % contentRefs.Count].Id : (Guid?)null;

                faqItems.Add(new FaqItem
                {
                    Id = itemId,
                    TenantId = tenantId,
                    FaqId = faq.Id,
                    Question = $"How do I complete task {i:00} for {faq.Name}?",
                    ShortAnswer = $"Follow the steps for {faq.Name} task {i:00}.",
                    Answer = includeAnswer ? $"Detailed guidance for {faq.Name} task {i:00}." : null,
                    AdditionalInfo = includeAdditional ? $"Extra notes for {faq.Name} task {i:00}." : null,
                    CtaTitle = includeCta ? "Open task guide" : null,
                    CtaUrl = includeCta ? $"https://portal.basefaq.local/{faq.Id}/tasks/{i:00}" : null,
                    Sort = i,
                    VoteScore = random.Next(-5, 20),
                    AiConfidenceScore = random.Next(40, 100),
                    IsActive = i % 11 != 0,
                    ContentRefId = contentRefId
                });

                for (var v = 0; v < counts.VotesPerItem; v++)
                {
                    var like = v % 2 == 0;
                    votes.Add(new Vote
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        FaqItemId = itemId,
                        Like = like,
                        UserPrint = $"seed-userprint-{faq.Id:N}-{i:00}-{v:00}",
                        Ip = $"192.168.{i % 255}.{v + 10}",
                        UserAgent = userAgentSamples[(i + v) % userAgentSamples.Length],
                        UnLikeReason = like ? null : unlikeReasons[(i + v) % unlikeReasons.Length]
                    });
                }

                contentCursor++;
            }

            for (var t = 0; t < counts.TagsPerFaq; t++)
            {
                var tag = tags[(tagCursor + t) % tags.Count];
                faqTags.Add(new FaqTag
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    FaqId = faq.Id,
                    TagId = tag.Id
                });
            }

            for (var c = 0; c < counts.ContentRefsPerFaq; c++)
            {
                var contentRef = contentRefs[(contentCursor + c) % contentRefs.Count];
                faqContentRefs.Add(new FaqContentRef
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    FaqId = faq.Id,
                    ContentRefId = contentRef.Id
                });
            }

            tagCursor += counts.TagsPerFaq;
            contentCursor += counts.ContentRefsPerFaq;
        }

        dbContext.FaqItems.AddRange(faqItems);
        dbContext.FaqTags.AddRange(faqTags);
        dbContext.FaqContentRefs.AddRange(faqContentRefs);
        dbContext.SaveChanges();

        dbContext.Votes.AddRange(votes);
        dbContext.SaveChanges();
    }

    private static List<Tag> BuildTags(int count, Guid tenantId)
    {
        var tags = new List<Tag>(count);
        for (var i = 1; i <= count; i++)
        {
            tags.Add(new Tag
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Value = $"tag-{i:000}"
            });
        }

        return tags;
    }

    private static List<ContentRef> BuildContentRefs(
        int count,
        Guid tenantId,
        ContentRefKind[] contentKinds,
        Random random)
    {
        var contentRefs = new List<ContentRef>(count);
        for (var i = 1; i <= count; i++)
        {
            var kind = contentKinds[(i - 1) % contentKinds.Length];
            contentRefs.Add(new ContentRef
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Kind = kind,
                Locator = kind switch
                {
                    ContentRefKind.Web => $"https://www.example.com/docs/{i:000}",
                    ContentRefKind.Pdf => $"https://www.example.com/files/guide-{i:000}.pdf",
                    ContentRefKind.Video => $"https://www.example.com/videos/{i:000}",
                    ContentRefKind.Repository => $"repo://basefaq/faq/{i:000}",
                    ContentRefKind.Document => $"doc://kb/{i:000}",
                    ContentRefKind.Faq => $"faq://{i:000}",
                    ContentRefKind.FaqItem => $"faq-item://{i:000}",
                    ContentRefKind.Manual => $"manual://base/{i:000}",
                    _ => $"other://ref/{i:000}"
                },
                Label = $"Reference {i:000}",
                Scope = i % 3 == 0 ? $"Section {random.Next(1, 12)}" : null
            });
        }

        return contentRefs;
    }

    private static List<Faq.Common.Persistence.FaqDb.Entities.Faq> BuildFaqs(
        int count,
        Guid tenantId,
        FaqStatus[] statuses,
        FaqSortStrategy[] sortStrategies)
    {
        var languages = new[] { "en-US", "en-GB", "en-CA", "en-AU", "en-IE", "en-NZ" };
        var faqs = new List<Faq.Common.Persistence.FaqDb.Entities.Faq>(count);

        for (var i = 1; i <= count; i++)
        {
            var status = statuses[(i - 1) % statuses.Length];
            var sort = sortStrategies[(i - 1) % sortStrategies.Length];
            var ctaEnabled = i % 2 == 0;
            var ctaTarget = i % 3 == 0 ? CtaTarget.Blank : CtaTarget.Self;

            faqs.Add(new Faq.Common.Persistence.FaqDb.Entities.Faq
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = $"FAQ {i:000}",
                Language = languages[(i - 1) % languages.Length],
                Status = status,
                SortStrategy = sort,
                CtaEnabled = ctaEnabled,
                CtaTarget = ctaTarget
            });
        }

        return faqs;
    }
}
