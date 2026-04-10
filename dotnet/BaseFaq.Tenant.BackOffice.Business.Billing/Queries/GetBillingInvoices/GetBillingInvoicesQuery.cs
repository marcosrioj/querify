using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.Billing;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingInvoices;

public sealed class GetBillingInvoicesQuery : IRequest<PagedResultDto<BillingInvoiceDto>>
{
    public required Guid TenantId { get; set; }
    public required BillingInvoiceGetAllRequestDto Request { get; set; }
}
