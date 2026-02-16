using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantAiProviderList;

public class TenantAiProvidersGetListQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantAiProvidersGetListQuery, PagedResultDto<TenantAiProviderDto>>
{
    public async Task<PagedResultDto<TenantAiProviderDto>> Handle(TenantAiProvidersGetListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var query = dbContext.TenantAiProviders
            .AsNoTracking()
            .Include(x => x.AiProvider)
            .AsQueryable();

        if (request.Request.TenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == request.Request.TenantId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.UpdatedDate)
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
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

        return new PagedResultDto<TenantAiProviderDto>(totalCount, items);
    }
}