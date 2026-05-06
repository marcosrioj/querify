using Querify.Models.Tenant.Dtos.TenantConnection;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Queries.GetTenantConnection;

public sealed class TenantConnectionsGetTenantConnectionQuery : IRequest<TenantConnectionDto?>
{
    public required Guid Id { get; set; }
}