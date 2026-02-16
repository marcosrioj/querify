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

        var provider = await dbContext.AiProviders
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.AiProviderId, cancellationToken);
        if (provider is null)
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
            var existingForCommand = await dbContext.TenantAiProviders
                .Include(x => x.AiProvider)
                .FirstOrDefaultAsync(
                    x => x.TenantId == tenant.Id && x.AiProvider.Command == provider.Command,
                    cancellationToken);

            if (existingForCommand is null)
            {
                dbContext.TenantAiProviders.Add(new BaseFaq.Common.EntityFramework.Tenant.Entities.TenantAiProvider
                {
                    TenantId = tenant.Id,
                    AiProviderId = provider.Id,
                    AiProviderKey = request.AiProviderKey
                });
            }
            else
            {
                existingForCommand.AiProviderId = provider.Id;
                existingForCommand.AiProviderKey = request.AiProviderKey;
                dbContext.TenantAiProviders.Update(existingForCommand);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}