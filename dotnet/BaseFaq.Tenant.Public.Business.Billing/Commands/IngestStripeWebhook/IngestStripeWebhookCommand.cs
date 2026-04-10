using MediatR;

namespace BaseFaq.Tenant.Public.Business.Billing.Commands.IngestStripeWebhook;

public sealed class IngestStripeWebhookCommand : IRequest<bool>
{
    public required string PayloadJson { get; set; }
    public string? Signature { get; set; }
}
