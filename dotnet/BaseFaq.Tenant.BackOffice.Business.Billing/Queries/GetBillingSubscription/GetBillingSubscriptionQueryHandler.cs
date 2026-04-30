using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Tenant.Dtos.Billing;
using BaseFaq.Models.Tenant.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingSubscription;

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
            .Where(entry => entry.TenantId == request.TenantId)
            .Select(entry => new TenantSubscriptionDetailDto
            {
                Id = entry.Id,
                TenantId = entry.TenantId,
                PlanCode = entry.PlanCode,
                BillingInterval = entry.BillingInterval,
                Status = entry.Status,
                Currency = entry.Currency,
                CountryCode = entry.CountryCode,
                TrialEndsAtUtc = entry.TrialEndsAtUtc,
                CurrentPeriodStartUtc = entry.CurrentPeriodStartUtc,
                CurrentPeriodEndUtc = entry.CurrentPeriodEndUtc,
                GraceUntilUtc = entry.GraceUntilUtc,
                DefaultProvider = entry.DefaultProvider,
                CancelAtPeriodEnd = entry.CancelAtPeriodEnd,
                CancelledAtUtc = entry.CancelledAtUtc,
                LastEventCreatedAtUtc = entry.LastEventCreatedAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        var providerSubscriptions = await dbContext.BillingProviderSubscriptions
            .AsNoTracking()
            .Where(entry => entry.TenantId == request.TenantId)
            .OrderByDescending(entry => entry.LastEventCreatedAtUtc ?? entry.UpdatedDate ?? entry.CreatedDate)
            .Select(entry => new BillingProviderSubscriptionDto
            {
                Id = entry.Id,
                TenantId = entry.TenantId,
                TenantSubscriptionId = entry.TenantSubscriptionId,
                Provider = entry.Provider,
                ExternalSubscriptionId = entry.ExternalSubscriptionId,
                ExternalPriceId = entry.ExternalPriceId,
                ExternalProductId = entry.ExternalProductId,
                Status = entry.Status,
                CurrentPeriodStartUtc = entry.CurrentPeriodStartUtc,
                CurrentPeriodEndUtc = entry.CurrentPeriodEndUtc,
                TrialEndsAtUtc = entry.TrialEndsAtUtc,
                CancelAtPeriodEnd = entry.CancelAtPeriodEnd,
                CancelledAtUtc = entry.CancelledAtUtc,
                LastEventCreatedAtUtc = entry.LastEventCreatedAtUtc,
                CreatedDateUtc = entry.CreatedDate,
                UpdatedDateUtc = entry.UpdatedDate
            })
            .ToListAsync(cancellationToken);

        if (subscription is null)
        {
            return new TenantSubscriptionDetailDto
            {
                TenantId = request.TenantId,
                DefaultProvider = BillingProviderType.Unknown,
                Status = TenantSubscriptionStatus.Unknown,
                ProviderSubscriptions = providerSubscriptions
            };
        }

        subscription.ProviderSubscriptions = providerSubscriptions;
        return subscription;
    }
}
