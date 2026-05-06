namespace Querify.Tenant.Public.Business.Billing.Abstractions;

public interface IBillingWebhookService
{
    Task IngestStripeWebhookAsync(string payloadJson, string? stripeSignature, CancellationToken cancellationToken);
}
