using System.Net;
using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Common.EntityFramework.Tenant.Abstractions;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Billing.Commands.RecomputeTenantEntitlements;

public sealed class RecomputeTenantEntitlementsCommandHandler(
    TenantDbContext dbContext,
    ITenantEntitlementSynchronizer entitlementSynchronizer)
    : IRequestHandler<RecomputeTenantEntitlementsCommand, Guid>
{
    public async Task<Guid> Handle(
        RecomputeTenantEntitlementsCommand request,
        CancellationToken cancellationToken)
    {
        var tenantExists = await dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(entry => entry.Id == request.TenantId, cancellationToken);
        if (!tenantExists)
        {
            throw new ApiErrorException(
                $"Tenant '{request.TenantId}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        await entitlementSynchronizer.SynchronizeAsync(request.TenantId, cancellationToken);
        return request.TenantId;
    }
}
