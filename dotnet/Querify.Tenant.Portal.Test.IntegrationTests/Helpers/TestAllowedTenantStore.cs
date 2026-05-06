using Querify.Common.Infrastructure.Core.Abstractions;

namespace Querify.Tenant.Portal.Test.IntegrationTests.Helpers;

public sealed class TestAllowedTenantStore : IAllowedTenantStore
{
    private readonly Dictionary<Guid, IReadOnlyDictionary<string, IReadOnlyCollection<Guid>>> _entries = [];

    public Task<IReadOnlyDictionary<string, IReadOnlyCollection<Guid>>?> GetAllowedTenantIds(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _entries.TryGetValue(userId, out var value);
        return Task.FromResult(value);
    }

    public Task SetAllowedTenantIds(
        Guid userId,
        IReadOnlyDictionary<string, IReadOnlyCollection<Guid>> tenantIds,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        _entries[userId] = tenantIds;
        return Task.CompletedTask;
    }

    public Task RemoveAllowedTenantIds(Guid userId, CancellationToken cancellationToken = default)
    {
        _entries.Remove(userId);
        return Task.CompletedTask;
    }
}
