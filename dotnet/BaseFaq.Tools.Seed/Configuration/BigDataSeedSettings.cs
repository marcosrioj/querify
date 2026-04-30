using Microsoft.Extensions.Configuration;

namespace BaseFaq.Tools.Seed.Configuration;

public sealed record BigDataSeedSettings(
    int SpaceCount,
    int QuestionsPerSpace,
    int TagsPerSpace,
    int SourcesPerSpace,
    int ActivitiesPerQuestion)
{
    private const int MinimumCount = 1;

    public static BigDataSeedSettings Default => new(
        SpaceCount: 20,
        QuestionsPerSpace: 10_000,
        TagsPerSpace: 2,
        SourcesPerSpace: 5,
        ActivitiesPerQuestion: 10);

    public long QuestionCount => (long)SpaceCount * QuestionsPerSpace;
    public long AnswerCount => QuestionCount;
    public long ActivityCount => QuestionCount * ActivitiesPerQuestion;
    public long TagCount => (long)SpaceCount * TagsPerSpace;
    public long SourceCount => (long)SpaceCount * SourcesPerSpace;

    public long EstimatedRowCount =>
        SpaceCount +
        TagCount +
        SourceCount +
        ((long)SpaceCount * TagsPerSpace) +
        ((long)SpaceCount * SourcesPerSpace) +
        QuestionCount +
        AnswerCount +
        QuestionCount +
        QuestionCount +
        QuestionCount +
        ActivityCount;

    public static BigDataSeedSettings From(IConfiguration configuration)
    {
        var defaults = Default;

        return new BigDataSeedSettings(
            SpaceCount: GetPositiveInt(configuration, "Seed:BigData:SpaceCount", defaults.SpaceCount),
            QuestionsPerSpace: GetPositiveInt(configuration, "Seed:BigData:QuestionsPerSpace", defaults.QuestionsPerSpace),
            TagsPerSpace: GetPositiveInt(configuration, "Seed:BigData:TagsPerSpace", defaults.TagsPerSpace),
            SourcesPerSpace: GetPositiveInt(configuration, "Seed:BigData:SourcesPerSpace", defaults.SourcesPerSpace),
            ActivitiesPerQuestion: GetPositiveInt(configuration, "Seed:BigData:ActivitiesPerQuestion", defaults.ActivitiesPerQuestion));
    }

    private static int GetPositiveInt(IConfiguration configuration, string key, int defaultValue)
    {
        var value = configuration[key];
        return int.TryParse(value, out var parsed)
            ? Math.Max(MinimumCount, parsed)
            : defaultValue;
    }
}
