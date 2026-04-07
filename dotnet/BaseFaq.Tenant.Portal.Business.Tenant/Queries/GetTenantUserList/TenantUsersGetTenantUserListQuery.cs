using BaseFaq.Models.Tenant.Dtos.TenantUser;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Queries.GetTenantUserList;

public sealed class TenantUsersGetTenantUserListQuery : IRequest<List<TenantUserDto>>;
