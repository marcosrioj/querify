using Querify.Models.Tenant.Dtos.Billing;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Billing.Queries.GetBillingWebhookInbox;

public sealed class GetBillingWebhookInboxQuery : IRequest<BillingWebhookInboxDetailDto?>
{
    public required Guid Id { get; set; }
}
