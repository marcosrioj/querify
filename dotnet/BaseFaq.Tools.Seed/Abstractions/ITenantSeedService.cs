using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Tools.Seed.Configuration;

namespace BaseFaq.Tools.Seed.Abstractions;

public interface ITenantSeedService
{
    bool HasData(TenantDbContext dbContext);
    bool HasEssentialData(TenantDbContext dbContext);
    Guid SeedDummyData(TenantDbContext dbContext, TenantSeedRequest request, SeedCounts counts);
    Guid EnsureEssentialData(TenantDbContext dbContext);
}