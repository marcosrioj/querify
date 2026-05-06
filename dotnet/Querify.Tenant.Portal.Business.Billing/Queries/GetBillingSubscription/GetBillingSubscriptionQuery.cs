using Querify.Models.Tenant.Dtos.Billing;
using MediatR;

namespace Querify.Tenant.Portal.Business.Billing.Queries.GetBillingSubscription;

public sealed class GetBillingSubscriptionQuery : IRequest<TenantSubscriptionDetailDto?>
{
    public required Guid TenantId { get; set; }
}
