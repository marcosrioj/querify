using BaseFaq.Models.Tenant.Dtos.Billing;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingSummary;

public sealed class GetBillingSummaryQuery : IRequest<TenantBillingSummaryDto?>
{
    public required Guid TenantId { get; set; }
}
