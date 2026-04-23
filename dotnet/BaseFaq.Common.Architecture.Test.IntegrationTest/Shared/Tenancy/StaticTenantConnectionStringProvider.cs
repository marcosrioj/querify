using BaseFaq.Common.Infrastructure.Core.Abstractions;

namespace BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Tenancy;

public sealed class StaticTenantConnectionStringProvider(string connectionString) : ITenantConnectionStringProvider
{
    public string ConnectionString { get; } = connectionString;

    public string GetConnectionString(Guid tenantId)
    {
        return ConnectionString;
    }
}
