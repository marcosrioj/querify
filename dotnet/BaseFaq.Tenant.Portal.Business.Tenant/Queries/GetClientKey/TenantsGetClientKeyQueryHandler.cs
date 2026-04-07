using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using System.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetClientKey;

public class TenantsGetClientKeyQueryHandler(TenantDbContext dbContext, ISessionService sessionService)
    : IRequestHandler<TenantsGetClientKeyQuery, string?>
{
    public async Task<string?> Handle(TenantsGetClientKeyQuery request, CancellationToken cancellationToken)
    {
        var tenantId = sessionService.GetTenantId(AppEnum.Faq);

        var tenant = await dbContext.Tenants
            .AsNoTracking()
            .Where(entity => entity.Id == tenantId && entity.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant is null)
        {
            throw new ApiErrorException(
                $"Tenant '{tenantId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return tenant.ClientKey;
    }
}
