using BaseFaq.Models.Tenant.Dtos.TenantConnection;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantConnection;

public sealed class TenantConnectionsGetTenantConnectionQuery : IRequest<TenantConnectionDto?>
{
    public required Guid Id { get; set; }
}