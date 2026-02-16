using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.SetAiProviderCredentials;

public class TenantsSetAiProviderCredentialsCommandHandler(TenantDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TenantsSetAiProviderCredentialsCommand>
{
    public async Task Handle(TenantsSetAiProviderCredentialsCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.AiProviderKey))
        {
            throw new ArgumentException("AiProviderKey is required.", nameof(request.AiProviderKey));
        }

        var userId = sessionService.GetUserId();

        var providerExists = await dbContext.AiProviders
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.AiProviderId, cancellationToken);

        if (!providerExists)
        {
            throw new ApiErrorException(
                $"AI Provider '{request.AiProviderId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        var tenants = await dbContext.Tenants
            .Where(x => x.UserId == userId && x.IsActive)
            .ToListAsync(cancellationToken);

        if (tenants.Count == 0)
        {
            throw new ApiErrorException(
                "Active tenant was not found for current user.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        foreach (var tenant in tenants)
        {
            tenant.AiProviderId = request.AiProviderId;
            tenant.AiProviderKey = request.AiProviderKey;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}