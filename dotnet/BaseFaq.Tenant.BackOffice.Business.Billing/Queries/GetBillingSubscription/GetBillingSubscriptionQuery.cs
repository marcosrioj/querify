using BaseFaq.Models.Tenant.Dtos.Billing;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Billing.Queries.GetBillingSubscription;

public sealed class GetBillingSubscriptionQuery : IRequest<TenantSubscriptionDetailDto?>
{
    public required Guid TenantId { get; set; }
}
