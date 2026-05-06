using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Tenant.Worker.Business.Billing.Abstractions;
using Querify.Tenant.Worker.Business.Billing.Models;
using System.Net;

namespace Querify.Tenant.Worker.Business.Billing.Services;

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
            throw new ApiErrorException(
                $"No billing webhook event handler is registered for '{billingEvent.Kind}'.",
                (int)HttpStatusCode.UnprocessableEntity);
        }

        await handler.HandleAsync(billingEvent, cancellationToken);
    }
}
