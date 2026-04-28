using BaseFaq.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

internal static class SpaceTagTenantIntegrityExtension
{
    internal static void EnsureSpaceTagTenantIntegrity(
        this QnADbContext dbContext,
        TenantIntegrityLookupCache cache)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<SpaceTag>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var link = entry.Entity;
            TenantIntegrityGuard.EnsureTenantMatch(
                link.TenantId,
                cache.GetSpaceTenant(link.SpaceId),
                nameof(SpaceTag.SpaceId));
            TenantIntegrityGuard.EnsureTenantMatch(
                link.TenantId,
                cache.GetTagTenant(link.TagId),
                nameof(SpaceTag.TagId));
        }
    }
}
