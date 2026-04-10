using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Models.Tenant.Dtos.Billing;

public sealed class BillingProviderSubscriptionDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid TenantSubscriptionId { get; set; }
    public BillingProviderType Provider { get; set; }
    public string ExternalSubscriptionId { get; set; } = string.Empty;
    public string? ExternalPriceId { get; set; }
    public string? ExternalProductId { get; set; }
    public TenantSubscriptionStatus Status { get; set; }
    public DateTime? CurrentPeriodStartUtc { get; set; }
    public DateTime? CurrentPeriodEndUtc { get; set; }
    public DateTime? TrialEndsAtUtc { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public DateTime? LastEventCreatedAtUtc { get; set; }
    public DateTime? CreatedDateUtc { get; set; }
    public DateTime? UpdatedDateUtc { get; set; }
}
