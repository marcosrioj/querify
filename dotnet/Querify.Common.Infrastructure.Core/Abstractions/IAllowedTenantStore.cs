namespace Querify.Common.Infrastructure.Core.Abstractions;

public interface IAllowedTenantStore
{
    Task<IReadOnlyDictionary<string, IReadOnlyCollection<Guid>>?> GetAllowedTenantIds(Guid userId,
        CancellationToken cancellationToken = default);

    Task SetAllowedTenantIds(Guid userId, IReadOnlyDictionary<string, IReadOnlyCollection<Guid>> tenantIds,
        TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    Task RemoveAllowedTenantIds(Guid userId, CancellationToken cancellationToken = default);
}