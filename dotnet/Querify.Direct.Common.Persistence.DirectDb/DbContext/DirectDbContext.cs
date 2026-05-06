using Querify.Common.EntityFramework.Core;
using Querify.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Direct.Common.Persistence.DirectDb.DbContext.TenantIntegrity;
using Querify.Direct.Common.Persistence.DirectDb.Entities;
using Querify.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ConversationMessageEntity = Querify.Direct.Common.Persistence.DirectDb.Entities.ConversationMessage;

namespace Querify.Direct.Common.Persistence.DirectDb.DbContext;

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
        "Querify.Direct.Common.Persistence.DirectDb.Configurations"
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
