using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Tenant.Dtos.AiProvider;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.AiProvider.Queries.GetAiProviderList;

public class AiProvidersGetAiProviderListQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<AiProvidersGetAiProviderListQuery, List<AiProviderDto>>
{
    public async Task<List<AiProviderDto>> Handle(
        AiProvidersGetAiProviderListQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.AiProviders
            .AsNoTracking()
            .OrderBy(x => x.Command)
            .ThenBy(x => x.Provider)
            .ThenBy(x => x.Model)
            .Select(x => new AiProviderDto
            {
                Id = x.Id,
                Provider = x.Provider,
                Model = x.Model,
                Prompt = x.Prompt,
                Command = x.Command
            })
            .ToListAsync(cancellationToken);
    }
}