using MediatR;

namespace Querify.Tenant.Portal.Business.Tenant.Queries.GetClientKey;

public sealed class TenantsGetClientKeyQuery : IRequest<string?>
{
    public required Guid TenantId { get; set; }
}
