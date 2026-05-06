using Querify.Tenant.Worker.Business.Billing.Models;

namespace Querify.Tenant.Worker.Business.Billing.Abstractions;

public interface IBillingWebhookEventHandler
{
    BillingWebhookEventKind Kind { get; }

    Task HandleAsync(BillingWebhookEvent billingEvent, CancellationToken cancellationToken = default);
}
