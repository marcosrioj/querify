using Querify.Models.Tenant.Dtos.Billing;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Billing.Queries.GetBillingSummary;

public sealed class GetBillingSummaryQuery : IRequest<TenantBillingSummaryDto?>
{
    public required Guid TenantId { get; set; }
}
