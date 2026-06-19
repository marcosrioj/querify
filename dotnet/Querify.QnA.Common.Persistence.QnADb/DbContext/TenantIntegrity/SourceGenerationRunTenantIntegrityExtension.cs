using Microsoft.EntityFrameworkCore;
using Querify.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using Querify.QnA.Common.Domain.Entities;

namespace Querify.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

internal static class SourceGenerationRunTenantIntegrityExtension
{
    internal static void EnsureSourceGenerationRunTenantIntegrity(
        this QnADbContext dbContext,
        TenantIntegrityLookupCache cache)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<SourceGenerationRun>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var run = entry.Entity;
            TenantIntegrityGuard.EnsureTenantMatch(
                run.TenantId,
                cache.GetSourceTenant(run.SourceId),
                nameof(SourceGenerationRun.SourceId));

            if (run.CreatedSpaceId.HasValue)
                TenantIntegrityGuard.EnsureTenantMatch(
                    run.TenantId,
                    cache.GetSpaceTenant(run.CreatedSpaceId.Value),
                    nameof(SourceGenerationRun.CreatedSpaceId));
        }
    }
}
