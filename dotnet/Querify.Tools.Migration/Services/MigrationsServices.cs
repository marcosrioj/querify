using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;

namespace Querify.Tools.Migration.Services;

internal sealed class MigrationsSessionService : ISessionService
{
    private static readonly Guid MigrationTenantId = Guid.Empty;
    private static readonly Guid MigrationUserId = Guid.Empty;

    public Guid GetTenantId(ModuleEnum module) => MigrationTenantId;

    public Guid GetUserId() => MigrationUserId;

    public string? GetUserName() => "migration";
}

internal sealed class NoopTenantConnectionStringProvider : ITenantConnectionStringProvider
{
    public string GetConnectionString(Guid tenantId)
    {
        throw new InvalidOperationException(
            "Tenant connection string provider is not available for design-time migrations.");
    }
}
