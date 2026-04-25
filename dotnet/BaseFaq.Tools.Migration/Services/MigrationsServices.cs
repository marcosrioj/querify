using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;

namespace BaseFaq.Tools.Migration.Services;

internal sealed class MigrationsSessionService : ISessionService
{
    private static readonly Guid MigrationTenantId = Guid.Empty;
    private static readonly Guid MigrationUserId = Guid.Empty;

    public Guid GetTenantId(ModuleEnum module) => MigrationTenantId;

    public Guid GetUserId() => MigrationUserId;
}

internal sealed class NoopTenantConnectionStringProvider : ITenantConnectionStringProvider
{
    public string GetConnectionString(Guid tenantId)
    {
        throw new InvalidOperationException(
            "Tenant connection string provider is not available for design-time migrations.");
    }
}