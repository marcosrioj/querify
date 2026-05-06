using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.Tenant;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Queries.GetTenantList;

public sealed class TenantsGetTenantListQuery : IRequest<PagedResultDto<TenantDto>>
{
    public required TenantGetAllRequestDto Request { get; set; }
}