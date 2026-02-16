using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetConfiguredAiProviders;

public class TenantsGetConfiguredAiProvidersQueryHandler(TenantDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TenantsGetConfiguredAiProvidersQuery, List<TenantAiProviderDto>>
{
    public async Task<List<TenantAiProviderDto>> Handle(TenantsGetConfiguredAiProvidersQuery request,
        CancellationToken cancellationToken)
    {
        var userId = sessionService.GetUserId();

        var tenantIds = await dbContext.Tenants
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        return await dbContext.TenantAiProviders
            .AsNoTracking()
            .Include(x => x.AiProvider)
            .Where(x => tenantIds.Contains(x.TenantId))
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