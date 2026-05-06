using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.TenantConnection;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Queries.GetTenantConnectionList;

public sealed class TenantConnectionsGetTenantConnectionListQuery : IRequest<PagedResultDto<TenantConnectionDto>>
{
    public required TenantConnectionGetAllRequestDto Request { get; set; }
}