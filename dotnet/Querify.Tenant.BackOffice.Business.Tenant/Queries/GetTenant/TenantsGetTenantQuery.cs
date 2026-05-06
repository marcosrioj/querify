using Querify.Models.Tenant.Dtos.Tenant;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Queries.GetTenant;

public sealed class TenantsGetTenantQuery : IRequest<TenantDto?>
{
    public required Guid Id { get; set; }
}