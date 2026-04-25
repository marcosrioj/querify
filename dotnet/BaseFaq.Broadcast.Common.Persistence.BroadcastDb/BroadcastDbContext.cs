using BaseFaq.Common.EntityFramework.Core;
using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Entities;
using BaseFaq.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ThreadEntity = BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Entities.Thread;

namespace BaseFaq.Broadcast.Common.Persistence.BroadcastDb;

public class BroadcastDbContext : BaseDbContext<BroadcastDbContext>
{
    public BroadcastDbContext(
        DbContextOptions<BroadcastDbContext> options,
        ISessionService sessionService,
        IConfiguration configuration,
        ITenantConnectionStringProvider tenantConnectionStringProvider,
        IHttpContextAccessor httpContextAccessor)
        : base(
            options,
            sessionService,
            configuration,
            tenantConnectionStringProvider,
            httpContextAccessor)
    {
    }

    public DbSet<ThreadEntity> Threads { get; set; }
    public DbSet<Item> Items { get; set; }

    protected override IEnumerable<string> ConfigurationNamespaces =>
    [
        "BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Configurations"
    ];

    protected override AppEnum SessionApp => AppEnum.Broadcast;

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureTenantIntegrity();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        EnsureTenantIntegrity();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnsureTenantIntegrity();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void EnsureTenantIntegrity()
    {
        var cache = new IntegrityLookupCache(this);

        foreach (var entry in ChangeTracker.Entries<Item>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var item = entry.Entity;
            EnsureTenantMatch(
                item.TenantId,
                cache.GetThreadTenant(item.ThreadId),
                nameof(Item.ThreadId));
        }
    }

    private static void EnsureTenantMatch(Guid expectedTenantId, Guid actualTenantId, string relationshipName)
    {
        if (actualTenantId != expectedTenantId)
            throw new InvalidOperationException(
                $"Cross-tenant relationship detected for '{relationshipName}'. Expected tenant '{expectedTenantId}' but found '{actualTenantId}'.");
    }

    private sealed class IntegrityLookupCache(BroadcastDbContext dbContext)
    {
        private Dictionary<Guid, Guid>? _threadTenants;

        public Guid GetThreadTenant(Guid id)
        {
            return GetTenant<ThreadEntity>(id, nameof(ThreadEntity), ref _threadTenants);
        }

        private Guid GetTenant<TEntity>(Guid id, string entityName, ref Dictionary<Guid, Guid>? cache)
            where TEntity : BaseEntity, IMustHaveTenant
        {
            cache ??= SeedTenantCache<TEntity>();

            if (cache.TryGetValue(id, out var tenantId)) return tenantId;

            var databaseLookup = dbContext.Set<TEntity>()
                .IgnoreQueryFilters()
                .Where(entity => entity.Id == id)
                .Select(entity => new TenantLookup
                {
                    TenantId = entity.TenantId
                })
                .SingleOrDefault();

            if (databaseLookup is null)
                throw new InvalidOperationException($"Referenced {entityName} '{id}' was not found.");

            cache[id] = databaseLookup.TenantId;
            return databaseLookup.TenantId;
        }

        private Dictionary<Guid, Guid> SeedTenantCache<TEntity>()
            where TEntity : BaseEntity, IMustHaveTenant
        {
            var cache = new Dictionary<Guid, Guid>();

            foreach (var entry in dbContext.ChangeTracker.Entries<TEntity>()
                         .Where(entry => entry.State != EntityState.Deleted))
                cache[entry.Entity.Id] = entry.Entity.TenantId;

            return cache;
        }
    }

    private sealed class TenantLookup
    {
        public required Guid TenantId { get; init; }
    }
}
