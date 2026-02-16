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

        return await dbContext.TenantAiProviders
            .AsNoTracking()
            .Include(x => x.AiProvider)
            .Where(x => x.AiProvider.Command == request.Command)
            .Join(
                dbContext.Tenants.AsNoTracking().Where(t => t.UserId == userId && t.IsActive),
                tap => tap.TenantId,
                t => t.Id,
                (tap, _) => tap)
            .AnyAsync(x => !string.IsNullOrWhiteSpace(x.AiProviderKey), cancellationToken);
    }
}