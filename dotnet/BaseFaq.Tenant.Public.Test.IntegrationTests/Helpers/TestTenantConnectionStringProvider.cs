using BaseFaq.Common.Infrastructure.Core.Abstractions;

namespace BaseFaq.Tenant.Public.Test.IntegrationTests.Helpers;

public sealed class TestTenantConnectionStringProvider : ITenantConnectionStringProvider
{
    private readonly string _connectionString;

    public TestTenantConnectionStringProvider(string connectionString)
    {
        _connectionString = connectionString;
    }

    public string GetConnectionString(Guid tenantId)
    {
        return _connectionString;
    }
}
