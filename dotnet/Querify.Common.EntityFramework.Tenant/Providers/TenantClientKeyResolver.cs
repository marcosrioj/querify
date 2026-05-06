using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Querify.Common.EntityFramework.Tenant.Providers;

public sealed class TenantClientKeyResolver(TenantDbContext tenantDbContext) : ITenantClientKeyResolver
{
    public async Task<Guid> ResolveTenantId(string clientKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientKey))
        {
            throw new ApiErrorException(
                "Client key is required.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        var tenantId = await tenantDbContext.Tenants
            .AsNoTracking()
            .Where(entity => entity.IsActive && entity.ClientKey == clientKey)
            .Select(entity => (Guid?)entity.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!tenantId.HasValue)
        {
            throw new ApiErrorException(
                "Client key is invalid or inactive.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        return tenantId.Value;
    }
}