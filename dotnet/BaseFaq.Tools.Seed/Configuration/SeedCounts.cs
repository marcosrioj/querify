namespace BaseFaq.Tools.Seed.Configuration;

public sealed record SeedCounts(
    int UserCount,
    int TenantCount,
    int TenantConnectionsPerApp,
    int FaqCount,
    int ItemsPerFaq,
    int TagCount,
    int ContentRefCount,
    int TagsPerFaq,
    int ContentRefsPerFaq,
    int VotesPerItem)
{
    public static SeedCounts Default => new(
        UserCount: 1,
        TenantCount: 1,
        TenantConnectionsPerApp: 1,
        FaqCount: 8,
        ItemsPerFaq: 4,
        TagCount: 32,
        ContentRefCount: 32,
        TagsPerFaq: 4,
        ContentRefsPerFaq: 4,
        VotesPerItem: 6);
}
