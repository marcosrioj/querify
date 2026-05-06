using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.EntityFramework.Core.Entities;
using Querify.Models.Tenant.Enums;

namespace Querify.Common.EntityFramework.Tenant.Entities;

public class TenantSubscription : BaseEntity, IMustHaveTenant
{
    public const int MaxPlanCodeLength = 128;
    public const int MaxCurrencyLength = 8;
    public const int MaxCountryCodeLength = 8;

    public required Guid TenantId { get; set; }
    public string? PlanCode { get; set; }
    public BillingIntervalType BillingInterval { get; set; } = BillingIntervalType.Unknown;
    public TenantSubscriptionStatus Status { get; set; } = TenantSubscriptionStatus.Unknown;
    public string? Currency { get; set; }
    public string? CountryCode { get; set; }
    public DateTime? TrialEndsAtUtc { get; set; }
    public DateTime? CurrentPeriodStartUtc { get; set; }
    public DateTime? CurrentPeriodEndUtc { get; set; }
    public DateTime? GraceUntilUtc { get; set; }
    public BillingProviderType DefaultProvider { get; set; } = BillingProviderType.Unknown;
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public DateTime? LastEventCreatedAtUtc { get; set; }
    public ICollection<BillingProviderSubscription> ProviderSubscriptions { get; set; } = [];
    public ICollection<BillingInvoice> Invoices { get; set; } = [];
}
