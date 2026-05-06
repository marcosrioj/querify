using Querify.Common.EntityFramework.Core;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Querify.Trust.Common.Persistence.TrustDb.DbContext;

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
        "Querify.Trust.Common.Persistence.TrustDb.Configurations"
    ];

    protected override ModuleEnum SessionModule => ModuleEnum.Trust;
}
