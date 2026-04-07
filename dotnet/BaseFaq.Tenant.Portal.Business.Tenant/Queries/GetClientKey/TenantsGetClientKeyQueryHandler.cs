using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetClientKey;

public class TenantsGetClientKeyQueryHandler(
    ITenantPortalAccessService tenantPortalAccessService)
    : IRequestHandler<TenantsGetClientKeyQuery, string?>
{
    public async Task<string?> Handle(TenantsGetClientKeyQuery request, CancellationToken cancellationToken)
    {
        var tenant = await tenantPortalAccessService.GetAccessibleTenantAsync(request.TenantId, cancellationToken);

        return tenant.ClientKey;
    }
}
