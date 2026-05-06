using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.EntityFramework.Core.Entities;
using Querify.Models.Tenant.Enums;

namespace Querify.Common.EntityFramework.Tenant.Entities;

public class TenantEntitlementSnapshot : BaseEntity, IMustHaveTenant
{
    public const int MaxPlanCodeLength = 128;

    public required Guid TenantId { get; set; }
    public string? PlanCode { get; set; }
    public TenantSubscriptionStatus SubscriptionStatus { get; set; } = TenantSubscriptionStatus.Unknown;
    public bool IsActive { get; set; }
    public bool IsInGracePeriod { get; set; }
    public DateTime? EffectiveUntilUtc { get; set; }
    public string? FeatureJson { get; set; }
}
