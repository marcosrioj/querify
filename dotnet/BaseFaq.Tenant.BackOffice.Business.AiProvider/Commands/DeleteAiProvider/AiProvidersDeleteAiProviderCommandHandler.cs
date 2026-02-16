using BaseFaq.Common.EntityFramework.Tenant;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.AiProvider.Commands.DeleteAiProvider;

public class AiProvidersDeleteAiProviderCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<AiProvidersDeleteAiProviderCommand>
{
    public async Task Handle(AiProvidersDeleteAiProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await dbContext.AiProviders
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (provider is null)
        {
            return;
        }

        dbContext.AiProviders.Remove(provider);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}