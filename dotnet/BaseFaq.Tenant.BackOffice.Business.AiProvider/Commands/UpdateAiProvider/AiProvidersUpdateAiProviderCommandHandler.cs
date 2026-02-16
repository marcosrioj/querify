using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.AiProvider.Commands.UpdateAiProvider;

public class AiProvidersUpdateAiProviderCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<AiProvidersUpdateAiProviderCommand>
{
    public async Task Handle(AiProvidersUpdateAiProviderCommand request, CancellationToken cancellationToken)
    {
        var provider = await dbContext.AiProviders
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (provider is null)
        {
            throw new ApiErrorException(
                $"AI Provider '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        provider.Provider = request.Provider;
        provider.Model = request.Model;
        provider.Prompt = request.Prompt;
        provider.Command = request.Command;

        dbContext.AiProviders.Update(provider);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}