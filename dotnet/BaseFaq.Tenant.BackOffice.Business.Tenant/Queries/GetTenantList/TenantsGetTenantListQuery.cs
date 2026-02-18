using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.Tenant;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantList;

public sealed class TenantsGetTenantListQuery : IRequest<PagedResultDto<TenantDto>>
{
    public required TenantGetAllRequestDto Request { get; set; }
}