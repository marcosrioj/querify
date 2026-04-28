using BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Entities;
using BaseFaq.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using Microsoft.EntityFrameworkCore;
using Thread = BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Entities.Thread;

namespace BaseFaq.Broadcast.Common.Persistence.BroadcastDb.DbContext.TenantIntegrity;

internal static class ItemTenantIntegrityExtension
{
    internal static void EnsureItemTenantIntegrity(
        this BroadcastDbContext dbContext,
        TenantIntegrityLookupCacheBase cacheBase)
    {
        Dictionary<Guid, Guid>? threadTenants = null;

        foreach (var entry in dbContext.ChangeTracker.Entries<Item>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var item = entry.Entity;
            TenantIntegrityGuard.EnsureTenantMatch(
                item.TenantId,
                cacheBase.GetTenant<Thread>(item.ThreadId, ref threadTenants),
                nameof(Item.ThreadId));
        }
    }
}