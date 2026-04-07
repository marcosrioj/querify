using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.IsAiProviderKeyConfigured;

public class TenantsIsAiProviderKeyConfiguredQueryHandler(TenantDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TenantsIsAiProviderKeyConfiguredQuery, bool>
{
    public async Task<bool> Handle(TenantsIsAiProviderKeyConfiguredQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.Faq);

        return await dbContext.TenantAiProviders
            .AsNoTracking()
            .Include(x => x.AiProvider)
            .Where(x => x.TenantId == tenantId && x.AiProvider.Command == request.Command)
            .AnyAsync(x => !string.IsNullOrWhiteSpace(x.AiProviderKey), cancellationToken);
    }
}
