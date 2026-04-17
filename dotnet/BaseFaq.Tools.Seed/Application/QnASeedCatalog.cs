namespace BaseFaq.Tools.Seed.Application;

internal static partial class QnASeedCatalog
{
    public static IReadOnlyList<SeedSpaceDefinition> Build() =>
    [
        ..BuildGitHubSpaces(),
        ..BuildGoogleSpaces(),
        ..BuildAppleSpaces(),
        ..BuildSpotifySpaces(),
        ..BuildSlackSpaces(),
        ..BuildTsaSpaces(),
        ..BuildUspsSpaces(),
        ..BuildAirbnbSpaces()
    ];

    private static SeedSpaceDefinition Space(
        string name,
        IReadOnlyList<string> tags,
        IReadOnlyList<SeedQuestionDefinition> items)
    {
        return new SeedSpaceDefinition(name, tags, items);
    }

    private static SeedQuestionDefinition Item(
        string question,
        string shortAnswer,
        string answer,
        SeedSource source,
        int helpfulFeedbackPercent,
        int confidenceScore)
    {
        return new SeedQuestionDefinition(
            question,
            shortAnswer,
            answer,
            source.SourceName,
            source.SourceLabel,
            source.SourceUrl,
            helpfulFeedbackPercent,
            confidenceScore);
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

internal sealed record SeedSpaceDefinition(
    string Name,
    IReadOnlyList<string> Tags,
    IReadOnlyList<SeedQuestionDefinition> Items);

internal sealed record SeedQuestionDefinition(
    string Question,
    string ShortAnswer,
    string Answer,
    string SourceName,
    string SourceLabel,
    string SourceUrl,
    int HelpfulFeedbackPercent,
    int ConfidenceScore);
