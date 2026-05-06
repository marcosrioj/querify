using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenant;

public sealed class TenantsDeleteTenantCommand : IRequest
{
    public required Guid Id { get; set; }
}