using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenantConnection;

public sealed class TenantConnectionsDeleteTenantConnectionCommand : IRequest
{
    public required Guid Id { get; set; }
}