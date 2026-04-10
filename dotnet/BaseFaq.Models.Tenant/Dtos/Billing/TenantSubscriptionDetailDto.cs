using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Models.Tenant.Dtos.Billing;

public sealed class TenantSubscriptionDetailDto
{
    public Guid? Id { get; set; }
    public Guid TenantId { get; set; }
    public string? PlanCode { get; set; }
    public BillingIntervalType BillingInterval { get; set; }
    public TenantSubscriptionStatus Status { get; set; }
    public string? Currency { get; set; }
    public string? CountryCode { get; set; }
    public DateTime? TrialEndsAtUtc { get; set; }
    public DateTime? CurrentPeriodStartUtc { get; set; }
    public DateTime? CurrentPeriodEndUtc { get; set; }
    public DateTime? GraceUntilUtc { get; set; }
    public BillingProviderType DefaultProvider { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public DateTime? LastEventCreatedAtUtc { get; set; }
    public IReadOnlyList<BillingProviderSubscriptionDto> ProviderSubscriptions { get; set; } =
        Array.Empty<BillingProviderSubscriptionDto>();
}
