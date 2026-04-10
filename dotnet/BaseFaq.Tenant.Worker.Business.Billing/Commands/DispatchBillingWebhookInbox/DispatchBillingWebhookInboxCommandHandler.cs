using MediatR;
using Microsoft.Extensions.Logging;

namespace BaseFaq.Tenant.Worker.Business.Billing.Commands.DispatchBillingWebhookInbox;

public sealed class DispatchBillingWebhookInboxCommandHandler(
    ILogger<DispatchBillingWebhookInboxCommandHandler> logger)
    : IRequestHandler<DispatchBillingWebhookInboxCommand>
{
    public Task Handle(DispatchBillingWebhookInboxCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workItem = command.WorkItem;

        logger.LogWarning(
            "Billing webhook inbox record {BillingWebhookInboxId} for provider {Provider} and event {EventType} cannot be dispatched because no billing webhook handler is implemented yet.",
            workItem.Id,
            workItem.Provider,
            workItem.EventType);

        throw new InvalidOperationException(
            $"No billing webhook handler is implemented for provider '{workItem.Provider}' and event type '{workItem.EventType}'.");
    }
}
