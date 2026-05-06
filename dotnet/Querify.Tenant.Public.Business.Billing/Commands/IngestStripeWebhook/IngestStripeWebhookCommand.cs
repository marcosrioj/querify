using MediatR;

namespace Querify.Tenant.Public.Business.Billing.Commands.IngestStripeWebhook;

public sealed class IngestStripeWebhookCommand : IRequest<bool>
{
    public required string PayloadJson { get; set; }
    public string? Signature { get; set; }
}
