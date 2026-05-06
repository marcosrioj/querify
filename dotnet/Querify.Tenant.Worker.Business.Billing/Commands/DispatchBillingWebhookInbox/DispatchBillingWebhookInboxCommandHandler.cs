using Querify.Tenant.Worker.Business.Billing.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Querify.Tenant.Worker.Business.Billing.Commands.DispatchBillingWebhookInbox;

public sealed class DispatchBillingWebhookInboxCommandHandler(
    IBillingProviderResolver billingProviderResolver,
    IBillingWebhookDispatcher billingWebhookDispatcher,
    ILogger<DispatchBillingWebhookInboxCommandHandler> logger)
    : IRequestHandler<DispatchBillingWebhookInboxCommand>
{
    public async Task Handle(DispatchBillingWebhookInboxCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var workItem = command.WorkItem;
        var provider = billingProviderResolver.Resolve(workItem.Provider);
        var billingEvent = provider.Parse(workItem);

        logger.LogInformation(
            "Dispatching billing webhook inbox record {BillingWebhookInboxId} for provider {Provider}, external event {ExternalEventId}, and normalized kind {BillingWebhookEventKind}.",
            workItem.Id,
            workItem.Provider,
            workItem.ExternalEventId,
            billingEvent.Kind);

        await billingWebhookDispatcher.DispatchAsync(billingEvent, cancellationToken);
    }
}
