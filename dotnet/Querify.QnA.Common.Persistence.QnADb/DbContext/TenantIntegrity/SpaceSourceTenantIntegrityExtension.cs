using Querify.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

internal static class SpaceSourceTenantIntegrityExtension
{
    internal static void EnsureSpaceSourceTenantIntegrity(
        this QnADbContext dbContext,
        TenantIntegrityLookupCache cache)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<SpaceSource>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var link = entry.Entity;
            TenantIntegrityGuard.EnsureTenantMatch(
                link.TenantId,
                cache.GetSpaceTenant(link.SpaceId),
                nameof(SpaceSource.SpaceId));
            TenantIntegrityGuard.EnsureTenantMatch(
                link.TenantId,
                cache.GetSourceTenant(link.SourceId),
                nameof(SpaceSource.SourceId));
        }
    }
}
