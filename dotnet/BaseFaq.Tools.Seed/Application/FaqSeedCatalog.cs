namespace BaseFaq.Tools.Seed.Application;

internal static partial class FaqSeedCatalog
{
    public static IReadOnlyList<SeedFaqDefinition> Build() =>
    [
        ..BuildGitHubFaqs(),
        ..BuildGoogleFaqs(),
        ..BuildAppleFaqs(),
        ..BuildSpotifyFaqs(),
        ..BuildSlackFaqs(),
        ..BuildTsaFaqs(),
        ..BuildUspsFaqs(),
        ..BuildAirbnbFaqs()
    ];

    private static SeedFaqDefinition Faq(
        string name,
        IReadOnlyList<string> tags,
        IReadOnlyList<SeedFaqItemDefinition> items)
    {
        return new SeedFaqDefinition(name, tags, items);
    }

    private static SeedFaqItemDefinition Item(
        string question,
        string shortAnswer,
        string answer,
        SeedSource source,
        int helpfulVotePercent,
        int aiConfidenceScore)
    {
        return new SeedFaqItemDefinition(
            question,
            shortAnswer,
            answer,
            source.SourceName,
            source.SourceLabel,
            source.SourceUrl,
            helpfulVotePercent,
            aiConfidenceScore);
    }

    private static SeedSource Source(string sourceName, string sourceLabel, string sourceUrl)
    {
        return new SeedSource(sourceName, sourceLabel, sourceUrl);
    }
}

internal sealed record SeedSource(
    string SourceName,
    string SourceLabel,
    string SourceUrl);

internal sealed record SeedFaqDefinition(
    string Name,
    IReadOnlyList<string> Tags,
    IReadOnlyList<SeedFaqItemDefinition> Items);

internal sealed record SeedFaqItemDefinition(
    string Question,
    string ShortAnswer,
    string Answer,
    string SourceName,
    string SourceLabel,
    string SourceUrl,
    int HelpfulVotePercent,
    int AiConfidenceScore);
