using Querify.Common.EntityFramework.Tenant;
using Querify.Tenant.Portal.Business.Tenant.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Querify.Tenant.Portal.Business.Tenant.Queries.GetClientKey;

public class TenantsGetClientKeyQueryHandler(
    TenantDbContext dbContext,
    ITenantPortalAccessService tenantPortalAccessService)
    : IRequestHandler<TenantsGetClientKeyQuery, string?>
{
    public async Task<string?> Handle(TenantsGetClientKeyQuery request, CancellationToken cancellationToken)
    {
        await tenantPortalAccessService.EnsureAccessAsync(request.TenantId, cancellationToken);

        return await dbContext.Tenants
            .AsNoTracking()
            .Where(tenant => tenant.Id == request.TenantId)
            .Select(tenant => tenant.ClientKey)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
