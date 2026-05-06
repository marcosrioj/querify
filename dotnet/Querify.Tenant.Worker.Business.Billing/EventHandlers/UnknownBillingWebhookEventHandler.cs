using Querify.Tenant.Worker.Business.Billing.Abstractions;
using Querify.Tenant.Worker.Business.Billing.Models;
using Microsoft.Extensions.Logging;

namespace Querify.Tenant.Worker.Business.Billing.EventHandlers;

public sealed class UnknownBillingWebhookEventHandler(ILogger<UnknownBillingWebhookEventHandler> logger)
    : IBillingWebhookEventHandler
{
    public BillingWebhookEventKind Kind => BillingWebhookEventKind.Unknown;

    public Task HandleAsync(BillingWebhookEvent billingEvent, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Billing webhook inbox record {BillingWebhookInboxId} for event type {EventType} is not mapped to a supported normalized billing event yet. Marking it as processed without side effects.",
            billingEvent.InboxId,
            billingEvent.EventType);

        return Task.CompletedTask;
    }
}
