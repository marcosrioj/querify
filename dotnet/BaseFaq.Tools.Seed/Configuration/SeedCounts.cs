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
    int FeedbacksPerItem)
{
    public static SeedCounts Default => new(
        UserCount: 1,
        TenantCount: 1,
        TenantConnectionsPerApp: 1,
        FaqCount: 16,
        ItemsPerFaq: 10,
        TagCount: 64,
        ContentRefCount: 128,
        TagsPerFaq: 4,
        ContentRefsPerFaq: 8,
        FeedbacksPerItem: 6);
}
