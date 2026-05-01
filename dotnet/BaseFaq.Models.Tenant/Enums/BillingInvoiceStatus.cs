namespace BaseFaq.Models.Tenant.Enums;

public enum BillingInvoiceStatus
{
    Unknown = 1,
    Draft = 6,
    Open = 11,
    Paid = 16,
    Uncollectible = 21,
    Void = 26,
    Failed = 31
}
