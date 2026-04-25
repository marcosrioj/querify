using BaseFaq.Common.EntityFramework.Core;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.Trust.Common.Persistence.TrustDb;

public class TrustDbContext : BaseDbContext<TrustDbContext>
{
    public TrustDbContext(
        DbContextOptions<TrustDbContext> options,
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

    protected override IEnumerable<string> ConfigurationNamespaces =>
    [
        "BaseFaq.Trust.Common.Persistence.TrustDb.Configurations"
    ];

    protected override ModuleEnum SessionModule => ModuleEnum.Trust;

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
        //TODO
    }
}