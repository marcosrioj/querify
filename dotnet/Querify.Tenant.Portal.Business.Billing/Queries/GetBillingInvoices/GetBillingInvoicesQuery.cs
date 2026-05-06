using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.Billing;
using MediatR;

namespace Querify.Tenant.Portal.Business.Billing.Queries.GetBillingInvoices;

public sealed class GetBillingInvoicesQuery : IRequest<PagedResultDto<BillingInvoiceDto>>
{
    public required Guid TenantId { get; set; }
    public required BillingInvoiceGetAllRequestDto Request { get; set; }
}
