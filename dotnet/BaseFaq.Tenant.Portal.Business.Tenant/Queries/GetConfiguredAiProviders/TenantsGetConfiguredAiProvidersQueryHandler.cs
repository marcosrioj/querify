using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using BaseFaq.Tenant.Portal.Business.Tenant.Helpers;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetConfiguredAiProviders;

public class TenantsGetConfiguredAiProvidersQueryHandler(
    TenantDbContext dbContext,
    ISessionService sessionService)
    : IRequestHandler<TenantsGetConfiguredAiProvidersQuery, List<TenantAiProviderDto>>
{
    public async Task<List<TenantAiProviderDto>> Handle(TenantsGetConfiguredAiProvidersQuery request,
        CancellationToken cancellationToken)
    {
        var userId = sessionService.GetUserId();
        var tenantId = request.TenantId;
        await TenantAccessHelper.EnsureAccessAsync(dbContext, tenantId, userId, AppEnum.Faq, cancellationToken);

        return await dbContext.TenantAiProviders
            .AsNoTracking()
            .Include(x => x.AiProvider)
            .Where(x => x.TenantId == tenantId)
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
