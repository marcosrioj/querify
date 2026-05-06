using Querify.Models.Tenant.Enums;

namespace Querify.Models.Tenant.Dtos.Billing;

public sealed class TenantEntitlementSnapshotDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? PlanCode { get; set; }
    public TenantSubscriptionStatus SubscriptionStatus { get; set; }
    public bool IsActive { get; set; }
    public bool IsInGracePeriod { get; set; }
    public DateTime? EffectiveUntilUtc { get; set; }
    public string? FeatureJson { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
