using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Tenant.Dtos.Billing;
using BaseFaq.Models.Tenant.Enums;
using BaseFaq.Tenant.Portal.Business.Billing.Service;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Tenant.Portal.Business.Billing.Queries.GetBillingSummary;

public sealed class GetBillingSummaryQueryHandler(TenantDbContext dbContext)
    : IRequestHandler<GetBillingSummaryQuery, TenantBillingSummaryDto?>
{
    public async Task<TenantBillingSummaryDto?> Handle(
        GetBillingSummaryQuery request,
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

        var entitlement = await dbContext.TenantEntitlementSnapshots
            .AsNoTracking()
            .FirstOrDefaultAsync(entry => entry.TenantId == request.TenantId, cancellationToken);

        var lastInvoice = await dbContext.BillingInvoices
            .AsNoTracking()
            .Where(entry => entry.TenantId == request.TenantId)
            .OrderByDescending(entry => entry.PaidAtUtc ?? entry.UpdatedDate ?? entry.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

        var lastPayment = await dbContext.BillingPayments
            .AsNoTracking()
            .Where(entry => entry.TenantId == request.TenantId)
            .OrderByDescending(entry => entry.PaidAtUtc ?? entry.UpdatedDate ?? entry.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

        return new TenantBillingSummaryDto
        {
            TenantId = request.TenantId,
            CurrentPlanCode = subscription?.PlanCode,
            DefaultProvider = subscription?.DefaultProvider ?? BillingProviderType.Unknown,
            SubscriptionStatus = subscription?.Status ?? TenantSubscriptionStatus.Unknown,
            TrialEndsAtUtc = subscription?.TrialEndsAtUtc,
            CurrentPeriodStartUtc = subscription?.CurrentPeriodStartUtc,
            CurrentPeriodEndUtc = subscription?.CurrentPeriodEndUtc,
            GraceUntilUtc = subscription?.GraceUntilUtc,
            LastInvoice = lastInvoice is null ? null : BillingDtoMapper.ToInvoiceDto(lastInvoice),
            LastPayment = lastPayment is null ? null : BillingDtoMapper.ToPaymentDto(lastPayment),
            Entitlement = BillingDtoMapper.ToEntitlementDto(entitlement)
        };
    }
}
