using BaseFaq.Models.Tenant.Dtos.Billing;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingWebhookInbox;

public sealed class GetBillingWebhookInboxQuery : IRequest<BillingWebhookInboxDetailDto?>
{
    public required Guid Id { get; set; }
}
