using BaseFaq.Common.EntityFramework.Core;
using BaseFaq.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Direct.Common.Persistence.DirectDb.DbContext.TenantIntegrity;
using BaseFaq.Direct.Common.Persistence.DirectDb.Entities;
using BaseFaq.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ConversationMessageEntity = BaseFaq.Direct.Common.Persistence.DirectDb.Entities.ConversationMessage;

namespace BaseFaq.Direct.Common.Persistence.DirectDb.DbContext;

public class DirectDbContext : BaseDbContext<DirectDbContext>
{
    public DirectDbContext(
        DbContextOptions<DirectDbContext> options,
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
    public DbSet<ConversationMessageEntity> ConversationMessages { get; set; }

    protected override IEnumerable<string> ConfigurationNamespaces =>
    [
        "BaseFaq.Direct.Common.Persistence.DirectDb.Configurations"
    ];

    protected override ModuleEnum SessionModule => ModuleEnum.Direct;

    protected override void OnBeforeSaveChangesRules()
    {
        EnsureTenantIntegrity();
    }

    private void EnsureTenantIntegrity()
    {
        var cache = new TenantIntegrityLookupCacheBase(this);
        this.EnsureConversationMessageTenantIntegrity(cache);
    }
}
