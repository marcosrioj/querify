using BaseFaq.Common.EntityFramework.Core;
using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.SupportCopilot.Common.Persistence.SupportCopilotDb.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.SupportCopilot.Common.Persistence.SupportCopilotDb;

public class SupportCopilotDbContext : BaseDbContext<SupportCopilotDbContext>
{
    public SupportCopilotDbContext(
        DbContextOptions<SupportCopilotDbContext> options,
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

    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationMessage> ConversationMessages { get; set; }

    protected override IEnumerable<string> ConfigurationNamespaces =>
    [
        "BaseFaq.SupportCopilot.Common.Persistence.SupportCopilotDb.Configurations"
    ];

    protected override AppEnum SessionApp => AppEnum.SupportCopilot;

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

        foreach (var entry in ChangeTracker.Entries<ConversationMessage>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var message = entry.Entity;
            EnsureTenantMatch(
                message.TenantId,
                cache.GetConversationTenant(message.ConversationId),
                nameof(ConversationMessage.ConversationId));
        }
    }

    private static void EnsureTenantMatch(Guid expectedTenantId, Guid actualTenantId, string relationshipName)
    {
        if (actualTenantId != expectedTenantId)
            throw new InvalidOperationException(
                $"Cross-tenant relationship detected for '{relationshipName}'. Expected tenant '{expectedTenantId}' but found '{actualTenantId}'.");
    }

    private sealed class IntegrityLookupCache(SupportCopilotDbContext dbContext)
    {
        private Dictionary<Guid, Guid>? _conversationTenants;

        public Guid GetConversationTenant(Guid id)
        {
            return GetTenant<Conversation>(id, nameof(Conversation), ref _conversationTenants);
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
