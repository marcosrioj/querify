using Querify.Models.Tenant.Dtos.Billing;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Billing.Queries.GetBillingSubscription;

public sealed class GetBillingSubscriptionQuery : IRequest<TenantSubscriptionDetailDto?>
{
    public required Guid TenantId { get; set; }
}
