namespace BaseFaq.Tools.Seed.Configuration;

public sealed record SeedCounts(
    int UserCount,
    int TenantCount,
    int TenantConnectionsPerModule,
    int SpaceCount,
    int QuestionsPerSpace,
    int TagCount,
    int SourceCount,
    int TagsPerSpace,
    int SourcesPerSpace,
    int SignalsPerQuestion)
{
    public static SeedCounts Default => new(
        UserCount: 1,
        TenantCount: 1,
        TenantConnectionsPerModule: 1,
        SpaceCount: 16,
        QuestionsPerSpace: 10,
        TagCount: 64,
        SourceCount: 128,
        TagsPerSpace: 4,
        SourcesPerSpace: 8,
        SignalsPerQuestion: 6);
}