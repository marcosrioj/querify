using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.Billing;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Billing.Queries.GetBillingWebhookInboxList;

public sealed class GetBillingWebhookInboxListQuery : IRequest<PagedResultDto<BillingWebhookInboxDto>>
{
    public required BillingWebhookInboxGetAllRequestDto Request { get; set; }
}
