using BaseFaq.Models.Tenant.Dtos.Tenant;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenant;

public sealed class TenantsGetTenantQuery : IRequest<TenantDto?>
{
    public required Guid Id { get; set; }
}