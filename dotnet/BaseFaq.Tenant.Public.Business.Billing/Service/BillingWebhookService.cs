using BaseFaq.Tenant.Public.Business.Billing.Abstractions;
using BaseFaq.Tenant.Public.Business.Billing.Commands.IngestStripeWebhook;
using MediatR;

namespace BaseFaq.Tenant.Public.Business.Billing.Service;

public sealed class BillingWebhookService(IMediator mediator) : IBillingWebhookService
{
    public async Task IngestStripeWebhookAsync(
        string payloadJson,
        string? stripeSignature,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payloadJson);

        await mediator.Send(
            new IngestStripeWebhookCommand
            {
                PayloadJson = payloadJson,
                Signature = stripeSignature
            },
            cancellationToken);
    }
}
