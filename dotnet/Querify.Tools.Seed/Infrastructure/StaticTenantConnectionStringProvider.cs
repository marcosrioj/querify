using Querify.Common.Infrastructure.Core.Abstractions;

namespace Querify.Tools.Seed.Infrastructure;

public sealed class StaticTenantConnectionStringProvider(string connectionString) : ITenantConnectionStringProvider
{
    public string GetConnectionString(Guid tenantId)
    {
        return connectionString;
    }
}