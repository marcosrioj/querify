using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Models.Tenant.Dtos.Billing;

public sealed class TenantBillingSummaryDto
{
    public Guid TenantId { get; set; }
    public string? CurrentPlanCode { get; set; }
    public BillingProviderType DefaultProvider { get; set; }
    public TenantSubscriptionStatus SubscriptionStatus { get; set; }
    public DateTime? TrialEndsAtUtc { get; set; }
    public DateTime? CurrentPeriodStartUtc { get; set; }
    public DateTime? CurrentPeriodEndUtc { get; set; }
    public DateTime? GraceUntilUtc { get; set; }
    public BillingInvoiceDto? LastInvoice { get; set; }
    public BillingPaymentDto? LastPayment { get; set; }
    public TenantEntitlementSnapshotDto? Entitlement { get; set; }
}
