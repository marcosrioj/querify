using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.Billing;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Billing.Queries.GetBillingPayments;

public sealed class GetBillingPaymentsQuery : IRequest<PagedResultDto<BillingPaymentDto>>
{
    public required Guid TenantId { get; set; }
    public required BillingPaymentGetAllRequestDto Request { get; set; }
}
