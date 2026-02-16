using BaseFaq.Common.EntityFramework.Tenant;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.AiProvider.Commands.CreateAiProvider;

public class AiProvidersCreateAiProviderCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<AiProvidersCreateAiProviderCommand, Guid>
{
    public async Task<Guid> Handle(AiProvidersCreateAiProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = new BaseFaq.Common.EntityFramework.Tenant.Entities.AiProvider
        {
            Provider = request.Provider,
            Model = request.Model,
            Prompt = request.Prompt,
            Command = request.Command
        };

        await dbContext.AiProviders.AddAsync(provider, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return provider.Id;
    }
}