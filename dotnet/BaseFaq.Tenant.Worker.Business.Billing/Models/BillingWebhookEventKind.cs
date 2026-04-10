namespace BaseFaq.Tenant.Worker.Business.Billing.Models;

public enum BillingWebhookEventKind
{
    Unknown = 0,
    CheckoutCompleted = 1,
    SubscriptionCreated = 2,
    SubscriptionUpdated = 3,
    SubscriptionCanceled = 4,
    InvoicePaid = 5,
    InvoicePaymentFailed = 6
}
