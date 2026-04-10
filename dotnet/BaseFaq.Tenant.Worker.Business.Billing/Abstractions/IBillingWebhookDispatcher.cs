using BaseFaq.Tenant.Worker.Business.Billing.Models;

namespace BaseFaq.Tenant.Worker.Business.Billing.Abstractions;

public interface IBillingWebhookDispatcher
{
    Task DispatchAsync(BillingWebhookEvent billingEvent, CancellationToken cancellationToken = default);
}
