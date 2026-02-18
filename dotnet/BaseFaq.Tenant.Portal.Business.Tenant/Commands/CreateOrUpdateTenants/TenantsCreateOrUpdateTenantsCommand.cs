using BaseFaq.Models.Tenant.Enums;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.CreateOrUpdateTenants;

public sealed class TenantsCreateOrUpdateTenantsCommand : IRequest<bool>
{
    public required string Name { get; set; }
    public required TenantEdition Edition { get; set; }
}