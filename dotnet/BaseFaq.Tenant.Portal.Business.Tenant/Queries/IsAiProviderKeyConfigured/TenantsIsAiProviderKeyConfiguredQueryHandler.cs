using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.IsAiProviderKeyConfigured;

public class TenantsIsAiProviderKeyConfiguredQueryHandler(TenantDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TenantsIsAiProviderKeyConfiguredQuery, bool>
{
    public async Task<bool> Handle(TenantsIsAiProviderKeyConfiguredQuery request, CancellationToken cancellationToken)
    {
        var userId = sessionService.GetUserId();

        return await dbContext.Tenants
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive && x.AiProviderId == request.AiProviderId)
            .AnyAsync(x => !string.IsNullOrWhiteSpace(x.AiProviderKey), cancellationToken);
    }
}