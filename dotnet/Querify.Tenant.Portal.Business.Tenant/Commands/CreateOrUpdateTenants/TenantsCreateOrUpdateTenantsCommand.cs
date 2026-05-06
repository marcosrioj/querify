using Querify.Models.Tenant.Enums;
using MediatR;

namespace Querify.Tenant.Portal.Business.Tenant.Commands.CreateOrUpdateTenants;

public sealed class TenantsCreateOrUpdateTenantsCommand : IRequest<bool>
{
    public Guid? TenantId { get; set; }
    public required string Name { get; set; }
    public required TenantEdition Edition { get; set; }
}
