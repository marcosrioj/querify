namespace Querify.Common.Infrastructure.Core.Abstractions;

public interface ITenantConnectionStringProvider
{
    string GetConnectionString(Guid tenantId);
}