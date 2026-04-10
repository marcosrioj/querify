using BaseFaq.Common.EntityFramework.Tenant.Entities;
using BaseFaq.Models.Tenant.Dtos.Billing;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Tenant.BackOffice.Business.Billing.Service;

internal static class BillingDtoMapper
{
    public static BillingInvoiceDto ToInvoiceDto(BillingInvoice invoice)
    {
        return new BillingInvoiceDto
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
        };
    }

    public static BillingPaymentDto ToPaymentDto(BillingPayment payment)
    {
        return new BillingPaymentDto
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
        };
    }

    public static BillingProviderSubscriptionDto ToProviderSubscriptionDto(BillingProviderSubscription subscription)
    {
        return new BillingProviderSubscriptionDto
        {
            Id = subscription.Id,
            TenantId = subscription.TenantId,
            TenantSubscriptionId = subscription.TenantSubscriptionId,
            Provider = subscription.Provider,
            ExternalSubscriptionId = subscription.ExternalSubscriptionId,
            ExternalPriceId = subscription.ExternalPriceId,
            ExternalProductId = subscription.ExternalProductId,
            Status = subscription.Status,
            CurrentPeriodStartUtc = subscription.CurrentPeriodStartUtc,
            CurrentPeriodEndUtc = subscription.CurrentPeriodEndUtc,
            TrialEndsAtUtc = subscription.TrialEndsAtUtc,
            CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
            CancelledAtUtc = subscription.CancelledAtUtc,
            LastEventCreatedAtUtc = subscription.LastEventCreatedAtUtc,
            CreatedDateUtc = subscription.CreatedDate,
            UpdatedDateUtc = subscription.UpdatedDate
        };
    }

    public static TenantEntitlementSnapshotDto? ToEntitlementDto(TenantEntitlementSnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return null;
        }

        return new TenantEntitlementSnapshotDto
        {
            Id = snapshot.Id,
            TenantId = snapshot.TenantId,
            PlanCode = snapshot.PlanCode,
            SubscriptionStatus = snapshot.SubscriptionStatus,
            IsActive = snapshot.IsActive,
            IsInGracePeriod = snapshot.IsInGracePeriod,
            EffectiveUntilUtc = snapshot.EffectiveUntilUtc,
            FeatureJson = snapshot.FeatureJson,
            UpdatedAtUtc = snapshot.UpdatedDate
        };
    }

    public static TenantSubscriptionDetailDto ToSubscriptionDetailDto(
        Guid tenantId,
        TenantSubscription? subscription,
        IReadOnlyList<BillingProviderSubscriptionDto> providerSubscriptions)
    {
        if (subscription is null)
        {
            return new TenantSubscriptionDetailDto
            {
                TenantId = tenantId,
                DefaultProvider = BillingProviderType.Unknown,
                Status = TenantSubscriptionStatus.Unknown,
                ProviderSubscriptions = providerSubscriptions
            };
        }

        return new TenantSubscriptionDetailDto
        {
            Id = subscription.Id,
            TenantId = subscription.TenantId,
            PlanCode = subscription.PlanCode,
            BillingInterval = subscription.BillingInterval,
            Status = subscription.Status,
            Currency = subscription.Currency,
            CountryCode = subscription.CountryCode,
            TrialEndsAtUtc = subscription.TrialEndsAtUtc,
            CurrentPeriodStartUtc = subscription.CurrentPeriodStartUtc,
            CurrentPeriodEndUtc = subscription.CurrentPeriodEndUtc,
            GraceUntilUtc = subscription.GraceUntilUtc,
            DefaultProvider = subscription.DefaultProvider,
            CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
            CancelledAtUtc = subscription.CancelledAtUtc,
            LastEventCreatedAtUtc = subscription.LastEventCreatedAtUtc,
            ProviderSubscriptions = providerSubscriptions
        };
    }
}
