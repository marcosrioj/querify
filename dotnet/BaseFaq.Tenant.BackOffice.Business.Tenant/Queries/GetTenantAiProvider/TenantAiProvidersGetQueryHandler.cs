using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantAiProvider;

public class TenantAiProvidersGetQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantAiProvidersGetQuery, TenantAiProviderDto?>
{
    public async Task<TenantAiProviderDto?> Handle(TenantAiProvidersGetQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.TenantAiProviders
            .AsNoTracking()
            .Include(x => x.AiProvider)
            .Where(x => x.Id == request.Id)
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
            .FirstOrDefaultAsync(cancellationToken);
    }
}