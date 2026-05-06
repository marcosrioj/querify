using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.EntityFramework.Core.Entities;
using Querify.Models.Tenant.Enums;

namespace Querify.Common.EntityFramework.Tenant.Entities;

public class BillingProviderSubscription : BaseEntity, IMustHaveTenant
{
    public const int MaxExternalSubscriptionIdLength = 255;
    public const int MaxExternalPriceIdLength = 255;
    public const int MaxExternalProductIdLength = 255;

    public required Guid TenantSubscriptionId { get; set; }
    public TenantSubscription TenantSubscription { get; set; } = null!;

    public required Guid TenantId { get; set; }
    public BillingProviderType Provider { get; set; } = BillingProviderType.Unknown;
    public required string ExternalSubscriptionId { get; set; }
    public string? ExternalPriceId { get; set; }
    public string? ExternalProductId { get; set; }
    public TenantSubscriptionStatus Status { get; set; } = TenantSubscriptionStatus.Unknown;
    public DateTime? CurrentPeriodStartUtc { get; set; }
    public DateTime? CurrentPeriodEndUtc { get; set; }
    public DateTime? TrialEndsAtUtc { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public DateTime? LastEventCreatedAtUtc { get; set; }
    public string? RawSnapshotJson { get; set; }
}
