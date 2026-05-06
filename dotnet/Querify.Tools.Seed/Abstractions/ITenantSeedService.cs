using Querify.Common.EntityFramework.Tenant;
using Querify.Tools.Seed.Configuration;

namespace Querify.Tools.Seed.Abstractions;

public interface ITenantSeedService
{
    bool HasEssentialData(TenantDbContext dbContext, TenantSeedRequest request, SeedCounts counts);
    EssentialSeedResult EnsureEssentialData(TenantDbContext dbContext, TenantSeedRequest request, SeedCounts counts);
}
