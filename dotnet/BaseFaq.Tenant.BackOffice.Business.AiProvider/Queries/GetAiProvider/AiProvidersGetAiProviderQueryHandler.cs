using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Tenant.Dtos.AiProvider;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.AiProvider.Queries.GetAiProvider;

public class AiProvidersGetAiProviderQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<AiProvidersGetAiProviderQuery, AiProviderDto?>
{
    public async Task<AiProviderDto?> Handle(AiProvidersGetAiProviderQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.AiProviders
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new AiProviderDto
            {
                Id = x.Id,
                Provider = x.Provider,
                Model = x.Model,
                Prompt = x.Prompt,
                Command = x.Command
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}