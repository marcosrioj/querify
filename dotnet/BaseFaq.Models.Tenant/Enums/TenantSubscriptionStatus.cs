namespace BaseFaq.Models.Tenant.Enums;

public enum TenantSubscriptionStatus
{
    Unknown = 0,
    Trialing = 1,
    Active = 2,
    PastDue = 3,
    Unpaid = 4,
    Canceled = 5,
    Incomplete = 6,
    IncompleteExpired = 7,
    Paused = 8
}
