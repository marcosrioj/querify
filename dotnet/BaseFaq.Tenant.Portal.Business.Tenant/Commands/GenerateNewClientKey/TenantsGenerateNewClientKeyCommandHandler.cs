using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Tenant.Portal.Business.Tenant.Helpers;
using System.Net;
using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.GenerateNewClientKey;

public class TenantsGenerateNewClientKeyCommandHandler(TenantDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TenantsGenerateNewClientKeyCommand, string>
{
    public async Task<string> Handle(TenantsGenerateNewClientKeyCommand request, CancellationToken cancellationToken)
    {
        var userId = sessionService.GetUserId();
        var tenantId = sessionService.GetTenantId(AppEnum.Faq);
        await TenantAccessHelper.EnsureOwnerAsync(dbContext, tenantId, userId, cancellationToken);

        var tenant = await dbContext.Tenants
            .FirstOrDefaultAsync(entity => entity.Id == tenantId && entity.IsActive, cancellationToken);

        if (tenant is null)
        {
            throw new ApiErrorException(
                $"Tenant '{tenantId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        var clientKey = GenerateClientKey();
        tenant.ClientKey = clientKey;

        dbContext.Tenants.Update(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);

        return clientKey;
    }

    private static string GenerateClientKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
