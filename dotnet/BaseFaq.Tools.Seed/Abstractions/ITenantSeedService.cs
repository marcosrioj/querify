using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Tools.Seed.Configuration;

namespace BaseFaq.Tools.Seed.Abstractions;

public interface ITenantSeedService
{
    bool HasEssentialData(TenantDbContext dbContext, TenantSeedRequest request, SeedCounts counts);
    EssentialSeedResult EnsureEssentialData(TenantDbContext dbContext, TenantSeedRequest request, SeedCounts counts);
}
