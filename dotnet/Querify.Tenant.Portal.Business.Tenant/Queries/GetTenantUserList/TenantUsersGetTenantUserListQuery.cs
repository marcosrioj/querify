using Querify.Models.Tenant.Dtos.TenantUser;
using MediatR;

namespace Querify.Tenant.Portal.Business.Tenant.Queries.GetTenantUserList;

public sealed class TenantUsersGetTenantUserListQuery : IRequest<List<TenantUserDto>>
{
    public required Guid TenantId { get; set; }
}
