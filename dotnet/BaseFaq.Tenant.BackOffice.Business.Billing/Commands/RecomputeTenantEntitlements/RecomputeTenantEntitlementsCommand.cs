using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Billing.Commands.RecomputeTenantEntitlements;

public sealed class RecomputeTenantEntitlementsCommand : IRequest<Guid>
{
    public required Guid TenantId { get; set; }
}
