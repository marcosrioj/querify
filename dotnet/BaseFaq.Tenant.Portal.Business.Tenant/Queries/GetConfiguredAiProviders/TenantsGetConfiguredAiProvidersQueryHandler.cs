using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetConfiguredAiProviders;

public class TenantsGetConfiguredAiProvidersQueryHandler(
    TenantDbContext dbContext,
    ITenantPortalAccessService tenantPortalAccessService)
    : IRequestHandler<TenantsGetConfiguredAiProvidersQuery, List<TenantAiProviderDto>>
{
    public async Task<List<TenantAiProviderDto>> Handle(TenantsGetConfiguredAiProvidersQuery request,
        CancellationToken cancellationToken)
    {
        await tenantPortalAccessService.EnsureAccessAsync(request.TenantId, cancellationToken);

        return await dbContext.TenantAiProviders
            .AsNoTracking()
            .Include(x => x.AiProvider)
            .Where(x => x.TenantId == request.TenantId)
            .OrderBy(x => x.AiProvider.Command)
            .ThenBy(x => x.AiProvider.Provider)
            .ThenBy(x => x.AiProvider.Model)
            .Select(x => new TenantAiProviderDto
            {
                Id = x.Id,
                TenantId = x.TenantId,
                AiProviderId = x.AiProviderId,
                Provider = x.AiProvider.Provider,
                Model = x.AiProvider.Model,
                Command = x.AiProvider.Command,
                IsAiProviderKeyConfigured = !string.IsNullOrWhiteSpace(x.AiProviderKey)
            })
            .ToListAsync(cancellationToken);
    }
}
