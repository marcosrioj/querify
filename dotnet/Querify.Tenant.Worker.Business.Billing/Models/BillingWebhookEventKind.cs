namespace Querify.Tenant.Worker.Business.Billing.Models;

public enum BillingWebhookEventKind
{
    Unknown = 1,
    CheckoutCompleted = 6,
    SubscriptionCreated = 11,
    SubscriptionUpdated = 16,
    SubscriptionCanceled = 21,
    InvoicePaid = 26,
    InvoicePaymentFailed = 31
}
