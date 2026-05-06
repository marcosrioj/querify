using Querify.Common.Infrastructure.Core.Abstractions;

namespace Querify.Common.Architecture.Test.IntegrationTest.Shared.Tenancy;

public sealed class StaticTenantConnectionStringProvider(string connectionString) : ITenantConnectionStringProvider
{
    public string ConnectionString { get; } = connectionString;

    public string GetConnectionString(Guid tenantId)
    {
        return ConnectionString;
    }
}
