using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.AiProvider;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.AiProvider.Queries.GetAiProviderList;

public class AiProvidersGetAiProviderListQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<AiProvidersGetAiProviderListQuery, PagedResultDto<AiProviderDto>>
{
    public async Task<PagedResultDto<AiProviderDto>> Handle(
        AiProvidersGetAiProviderListQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        var query = dbContext.AiProviders.AsNoTracking();
        query = ApplySorting(query, request.Request.Sorting);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(request.Request.SkipCount)
            .Take(request.Request.MaxResultCount)
            .Select(x => new AiProviderDto
            {
                Id = x.Id,
                Provider = x.Provider,
                Model = x.Model,
                Prompt = x.Prompt,
                Command = x.Command
            })
            .ToListAsync(cancellationToken);

        return new PagedResultDto<AiProviderDto>(totalCount, items);
    }

    private static IQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.AiProvider> ApplySorting(
        IQueryable<BaseFaq.Common.EntityFramework.Tenant.Entities.AiProvider> query,
        string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query.OrderByDescending(x => x.UpdatedDate);
        }

        return sorting.Trim().ToLowerInvariant() switch
        {
            "provider" => query.OrderBy(x => x.Provider),
            "provider desc" => query.OrderByDescending(x => x.Provider),
            "model" => query.OrderBy(x => x.Model),
            "model desc" => query.OrderByDescending(x => x.Model),
            "command" => query.OrderBy(x => x.Command),
            "command desc" => query.OrderByDescending(x => x.Command),
            _ => query.OrderByDescending(x => x.UpdatedDate)
        };
    }
}