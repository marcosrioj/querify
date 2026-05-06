using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Microsoft.EntityFrameworkCore;
using System.Net;
using EfDbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace Querify.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;

public sealed class TenantIntegrityLookupCacheBase(EfDbContext dbContext)
{
    public Guid GetTenant<TEntity>(
        Guid id,
        ref Dictionary<Guid, Guid>? cache,
        string? entityName = null)
        where TEntity : class, IMustHaveTenant
    {
        cache ??= SeedTenantCache<TEntity>();

        if (cache.TryGetValue(id, out var tenantId)) return tenantId;

        var databaseTenantId = dbContext.Set<TEntity>()
            .IgnoreQueryFilters()
            .Where(entity => EF.Property<Guid>(entity, "Id") == id)
            .Select(entity => (Guid?)EF.Property<Guid>(entity, nameof(IMustHaveTenant.TenantId)))
            .SingleOrDefault();

        if (databaseTenantId is null)
            throw new ApiErrorException(
                $"Referenced {entityName ?? typeof(TEntity).Name} '{id}' was not found.",
                (int)HttpStatusCode.NotFound);

        cache[id] = databaseTenantId.Value;
        return databaseTenantId.Value;
    }

    private Dictionary<Guid, Guid> SeedTenantCache<TEntity>()
        where TEntity : class, IMustHaveTenant
    {
        var cache = new Dictionary<Guid, Guid>();

        foreach (var entry in dbContext.ChangeTracker.Entries<TEntity>()
                     .Where(entry => entry.State != EntityState.Deleted))
        {
            if (entry.Property("Id").CurrentValue is Guid id)
            {
                cache[id] = entry.Entity.TenantId;
            }
        }

        return cache;
    }
}
