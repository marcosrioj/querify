using System.Text.Json;
using Querify.Common.Infrastructure.Core.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace Querify.Common.Infrastructure.Core.Services;

public sealed class RedisAllowedTenantStore(IDistributedCache cache) : IAllowedTenantStore
{
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(30);

    public async Task<IReadOnlyDictionary<string, IReadOnlyCollection<Guid>>?> GetAllowedTenantIds(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var payload = await cache.GetStringAsync(BuildCacheKey(userId), cancellationToken);
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        return JsonSerializer.Deserialize<IReadOnlyDictionary<string, IReadOnlyCollection<Guid>>>(payload);
    }

    public Task SetAllowedTenantIds(Guid userId,
        IReadOnlyDictionary<string, IReadOnlyCollection<Guid>> tenantIds,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(tenantIds);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? DefaultCacheDuration
        };

        return cache.SetStringAsync(BuildCacheKey(userId), payload, options, cancellationToken);
    }

    public Task RemoveAllowedTenantIds(Guid userId, CancellationToken cancellationToken = default)
    {
        return cache.RemoveAsync(BuildCacheKey(userId), cancellationToken);
    }

    private static string BuildCacheKey(Guid userId)
    {
        return $"AllowedTenants:{userId}";
    }
}