using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.Billing;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingWebhookInboxList;

public sealed class GetBillingWebhookInboxListQuery : IRequest<PagedResultDto<BillingWebhookInboxDto>>
{
    public required BillingWebhookInboxGetAllRequestDto Request { get; set; }
}
