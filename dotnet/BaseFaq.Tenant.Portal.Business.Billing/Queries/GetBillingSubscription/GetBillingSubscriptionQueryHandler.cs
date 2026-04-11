using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Tenant.Dtos.Billing;
using BaseFaq.Tenant.Portal.Business.Billing.Service;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Billing.Queries.GetBillingSubscription;

public sealed class GetBillingSubscriptionQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<GetBillingSubscriptionQuery, TenantSubscriptionDetailDto?>
{
    public async Task<TenantSubscriptionDetailDto?> Handle(
        GetBillingSubscriptionQuery request,
        CancellationToken cancellationToken)
    {
        var tenantExists = await dbContext.Tenants
            .AsNoTracking()
            .AnyAsync(entry => entry.Id == request.TenantId, cancellationToken);
        if (!tenantExists)
        {
            return null;
        }

        var subscription = await dbContext.TenantSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.TenantId == request.TenantId, cancellationToken);

        var providerSubscriptions = await dbContext.BillingProviderSubscriptions
            .AsNoTracking()
            .Where(entry => entry.TenantId == request.TenantId)
            .OrderByDescending(entry => entry.LastEventCreatedAtUtc ?? entry.UpdatedDate ?? entry.CreatedDate)
            .ToListAsync(cancellationToken);

        return BillingDtoMapper.ToSubscriptionDetailDto(
            request.TenantId,
            subscription,
            providerSubscriptions.Select(BillingDtoMapper.ToProviderSubscriptionDto).ToList());
    }
}
