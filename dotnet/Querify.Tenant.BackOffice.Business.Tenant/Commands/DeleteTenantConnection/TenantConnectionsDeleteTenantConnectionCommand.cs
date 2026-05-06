using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenantConnection;

public sealed class TenantConnectionsDeleteTenantConnectionCommand : IRequest
{
    public required Guid Id { get; set; }
}