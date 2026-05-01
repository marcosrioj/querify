namespace BaseFaq.Models.Tenant.Enums;

public enum TenantSubscriptionStatus
{
    Unknown = 1,
    Trialing = 6,
    Active = 11,
    PastDue = 16,
    Unpaid = 21,
    Canceled = 26,
    Incomplete = 31,
    IncompleteExpired = 36,
    Paused = 41
}
