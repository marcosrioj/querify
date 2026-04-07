using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Common.EntityFramework.Tenant.Providers;

public sealed class AllowedTenantProvider(TenantDbContext tenantDbContext) : IAllowedTenantProvider
{
    public async Task<IReadOnlyDictionary<string, IReadOnlyCollection<Guid>>> GetAllowedTenantIds(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var tenants = await tenantDbContext.TenantUsers
            .AsNoTracking()
            .Where(entity => entity.UserId == userId && entity.Tenant.IsActive)
            .Select(entity => new { entity.Tenant.App, entity.TenantId })
            .Distinct()
            .ToListAsync(cancellationToken);

        var lookup = tenants
            .GroupBy(entity => entity.App.ToString())
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyCollection<Guid>)group.Select(entity => entity.TenantId).ToList());

        foreach (var appEnum in Enum.GetValues<AppEnum>())
        {
            var key = appEnum.ToString();
            lookup.TryAdd(key, Array.Empty<Guid>());
        }

        return lookup;
    }
}
