using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using System.Security.Cryptography;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.GenerateNewClientKey;

public class TenantsGenerateNewClientKeyCommandHandler(
    TenantDbContext dbContext,
    ITenantPortalAccessService tenantPortalAccessService)
    : IRequestHandler<TenantsGenerateNewClientKeyCommand, string>
{
    public async Task<string> Handle(TenantsGenerateNewClientKeyCommand request, CancellationToken cancellationToken)
    {
        var tenant = await tenantPortalAccessService.GetAccessibleTenantAsync(request.TenantId, cancellationToken);

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
