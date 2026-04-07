using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.IsAiProviderKeyConfigured;

public class TenantsIsAiProviderKeyConfiguredQueryHandler(
    TenantDbContext dbContext,
    ITenantPortalAccessService tenantPortalAccessService)
    : IRequestHandler<TenantsIsAiProviderKeyConfiguredQuery, bool>
{
    public async Task<bool> Handle(TenantsIsAiProviderKeyConfiguredQuery request, CancellationToken cancellationToken)
    {
        await tenantPortalAccessService.EnsureAccessAsync(request.TenantId, cancellationToken);

        return await dbContext.TenantAiProviders
            .AsNoTracking()
            .Include(x => x.AiProvider)
            .Where(x => x.TenantId == request.TenantId && x.AiProvider.Command == request.Command)
            .AnyAsync(x => !string.IsNullOrWhiteSpace(x.AiProviderKey), cancellationToken);
    }
}
