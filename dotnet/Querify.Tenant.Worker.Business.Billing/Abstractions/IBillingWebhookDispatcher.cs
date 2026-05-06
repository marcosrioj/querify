using Querify.Tenant.Worker.Business.Billing.Models;

namespace Querify.Tenant.Worker.Business.Billing.Abstractions;

public interface IBillingWebhookDispatcher
{
    Task DispatchAsync(BillingWebhookEvent billingEvent, CancellationToken cancellationToken = default);
}
