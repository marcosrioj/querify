using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenantAiProvider;

public class TenantAiProvidersCreateCommandHandler(TenantDbContext dbContext)
    : IRequestHandler<TenantAiProvidersCreateCommand, Guid>
{
    public async Task<Guid> Handle(TenantAiProvidersCreateCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AiProviderKey))
        {
            throw new ApiErrorException(
                "AiProviderKey is required.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        var tenantExists = await dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.TenantId, cancellationToken);
        if (!tenantExists)
        {
            throw new ApiErrorException($"Tenant '{request.TenantId}' was not found.",
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

        var existsByCommand = await dbContext.TenantAiProviders
            .Include(x => x.AiProvider)
            .AnyAsync(x => x.TenantId == request.TenantId && x.AiProvider.Command == provider.Command,
                cancellationToken);
        if (existsByCommand)
        {
            throw new ApiErrorException(
                $"Tenant '{request.TenantId}' already has an AI provider configured for command '{provider.Command}'.",
                errorCode: (int)HttpStatusCode.Conflict);
        }

        var entity = new BaseFaq.Common.EntityFramework.Tenant.Entities.TenantAiProvider
        {
            TenantId = request.TenantId,
            AiProviderId = request.AiProviderId,
            AiProviderKey = request.AiProviderKey
        };

        await dbContext.TenantAiProviders.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}