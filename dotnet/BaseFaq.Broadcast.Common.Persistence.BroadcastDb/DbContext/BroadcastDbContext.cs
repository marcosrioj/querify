using BaseFaq.Broadcast.Common.Persistence.BroadcastDb.DbContext.TenantIntegrity;
using BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Entities;
using BaseFaq.Common.EntityFramework.Core;
using BaseFaq.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Thread = System.Threading.Thread;

namespace BaseFaq.Broadcast.Common.Persistence.BroadcastDb.DbContext;

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

    public DbSet<Thread> Threads { get; set; }
    public DbSet<Item> Items { get; set; }

    protected override IEnumerable<string> ConfigurationNamespaces =>
    [
        "BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Configurations"
    ];

    protected override ModuleEnum SessionModule => ModuleEnum.Broadcast;

    protected override void OnBeforeSaveChangesRules()
    {
        EnsureTenantIntegrity();
    }

    private void EnsureTenantIntegrity()
    {
        var cache = new TenantIntegrityLookupCacheBase(this);
        this.EnsureItemTenantIntegrity(cache);
    }
}
