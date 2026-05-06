using MediatR;

namespace Querify.Tenant.BackOffice.Business.Billing.Commands.RequeueBillingWebhookInbox;

public sealed class RequeueBillingWebhookInboxCommand : IRequest<Guid>
{
    public required Guid Id { get; set; }
}
