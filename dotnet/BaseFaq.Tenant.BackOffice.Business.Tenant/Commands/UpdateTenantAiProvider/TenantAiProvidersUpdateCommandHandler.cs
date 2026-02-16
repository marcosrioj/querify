using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenantAiProvider;

public class TenantAiProvidersUpdateCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantAiProvidersUpdateCommand>
{
    public async Task Handle(TenantAiProvidersUpdateCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AiProviderKey))
        {
            throw new ApiErrorException(
                "AiProviderKey is required.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        var entity = await dbContext.TenantAiProviders
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (entity is null)
        {
            throw new ApiErrorException($"Tenant AI Provider '{request.Id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        var provider = await dbContext.AiProviders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.AiProviderId, cancellationToken);
        if (provider is null)
        {
            throw new ApiErrorException($"AI Provider '{request.AiProviderId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        var conflict = await dbContext.TenantAiProviders
            .Include(x => x.AiProvider)
            .AnyAsync(
                x => x.Id != entity.Id &&
                     x.TenantId == entity.TenantId &&
                     x.AiProvider.Command == provider.Command,
                cancellationToken);
        if (conflict)
        {
            throw new ApiErrorException(
                $"Tenant '{entity.TenantId}' already has an AI provider configured for command '{provider.Command}'.",
                errorCode: (int)HttpStatusCode.Conflict);
        }

        entity.AiProviderId = request.AiProviderId;
        entity.AiProviderKey = request.AiProviderKey;
        dbContext.TenantAiProviders.Update(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}