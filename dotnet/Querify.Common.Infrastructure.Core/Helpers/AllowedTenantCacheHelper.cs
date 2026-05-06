using Querify.Common.Infrastructure.Core.Abstractions;

namespace Querify.Common.Infrastructure.Core.Helpers;

public static class AllowedTenantCacheHelper
{
    public static Task RemoveUserEntries(
        IAllowedTenantStore allowedTenantStore,
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var cacheInvalidations = userIds
            .Where(userId => userId != Guid.Empty)
            .Distinct()
            .Select(userId => allowedTenantStore.RemoveAllowedTenantIds(userId, cancellationToken));

        return Task.WhenAll(cacheInvalidations);
    }
}
