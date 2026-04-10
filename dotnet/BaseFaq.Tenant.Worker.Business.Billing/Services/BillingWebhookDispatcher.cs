using BaseFaq.Tenant.Worker.Business.Billing.Abstractions;
using BaseFaq.Tenant.Worker.Business.Billing.Models;

namespace BaseFaq.Tenant.Worker.Business.Billing.Services;

public sealed class BillingWebhookDispatcher(IEnumerable<IBillingWebhookEventHandler> handlers)
    : IBillingWebhookDispatcher
{
    private readonly Dictionary<BillingWebhookEventKind, IBillingWebhookEventHandler> _handlers = handlers.ToDictionary(
        handler => handler.Kind,
        handler => handler);

    public async Task DispatchAsync(BillingWebhookEvent billingEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(billingEvent);

        if (!_handlers.TryGetValue(billingEvent.Kind, out var handler))
        {
            throw new InvalidOperationException(
                $"No billing webhook event handler is registered for '{billingEvent.Kind}'.");
        }

        await handler.HandleAsync(billingEvent, cancellationToken);
    }
}
