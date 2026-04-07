using BaseFaq.Models.Tenant.Dtos.TenantUser;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Queries.GetTenantUserList;

public sealed class TenantUsersGetTenantUserListQuery : IRequest<List<TenantUserDto>>
{
    public required Guid TenantId { get; set; }
}
