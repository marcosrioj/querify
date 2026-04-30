using BaseFaq.Common.EntityFramework.Tenant;
using BaseFaq.Models.Tenant.Dtos.Billing;
using BaseFaq.Models.Tenant.Enums;
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
            .Where(entry => entry.TenantId == request.TenantId)
            .Select(entry => new
            {
                entry.PlanCode,
                entry.DefaultProvider,
                entry.Status,
                entry.TrialEndsAtUtc,
                entry.CurrentPeriodStartUtc,
                entry.CurrentPeriodEndUtc,
                entry.GraceUntilUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        var entitlement = await dbContext.TenantEntitlementSnapshots
            .AsNoTracking()
            .Where(entry => entry.TenantId == request.TenantId)
            .Select(entry => new TenantEntitlementSnapshotDto
            {
                Id = entry.Id,
                TenantId = entry.TenantId,
                PlanCode = entry.PlanCode,
                SubscriptionStatus = entry.SubscriptionStatus,
                IsActive = entry.IsActive,
                IsInGracePeriod = entry.IsInGracePeriod,
                EffectiveUntilUtc = entry.EffectiveUntilUtc,
                FeatureJson = entry.FeatureJson,
                UpdatedAtUtc = entry.UpdatedDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        var lastInvoice = await dbContext.BillingInvoices
            .AsNoTracking()
            .Where(entry => entry.TenantId == request.TenantId)
            .OrderByDescending(entry => entry.PaidAtUtc ?? entry.UpdatedDate ?? entry.CreatedDate)
            .Select(invoice => new BillingInvoiceDto
            {
                Id = invoice.Id,
                TenantId = invoice.TenantId,
                TenantSubscriptionId = invoice.TenantSubscriptionId,
                Provider = invoice.Provider,
                ExternalInvoiceId = invoice.ExternalInvoiceId,
                AmountMinor = invoice.AmountMinor,
                Currency = invoice.Currency,
                DueDateUtc = invoice.DueDateUtc,
                PaidAtUtc = invoice.PaidAtUtc,
                Status = invoice.Status,
                HostedUrl = invoice.HostedUrl,
                PdfUrl = invoice.PdfUrl,
                CreatedDateUtc = invoice.CreatedDate,
                UpdatedDateUtc = invoice.UpdatedDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        var lastPayment = await dbContext.BillingPayments
            .AsNoTracking()
            .Where(entry => entry.TenantId == request.TenantId)
            .OrderByDescending(entry => entry.PaidAtUtc ?? entry.UpdatedDate ?? entry.CreatedDate)
            .Select(payment => new BillingPaymentDto
            {
                Id = payment.Id,
                TenantId = payment.TenantId,
                BillingInvoiceId = payment.BillingInvoiceId,
                Provider = payment.Provider,
                ExternalPaymentId = payment.ExternalPaymentId,
                Method = payment.Method,
                AmountMinor = payment.AmountMinor,
                Currency = payment.Currency,
                Status = payment.Status,
                FailureCode = payment.FailureCode,
                FailureMessage = payment.FailureMessage,
                PaidAtUtc = payment.PaidAtUtc,
                CreatedDateUtc = payment.CreatedDate,
                UpdatedDateUtc = payment.UpdatedDate
            })
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
            LastInvoice = lastInvoice,
            LastPayment = lastPayment,
            Entitlement = entitlement
        };
    }
}
