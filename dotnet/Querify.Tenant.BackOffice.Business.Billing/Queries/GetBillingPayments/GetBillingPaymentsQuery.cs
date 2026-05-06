using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.Billing;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Billing.Queries.GetBillingPayments;

public sealed class GetBillingPaymentsQuery : IRequest<PagedResultDto<BillingPaymentDto>>
{
    public required Guid TenantId { get; set; }
    public required BillingPaymentGetAllRequestDto Request { get; set; }
}
