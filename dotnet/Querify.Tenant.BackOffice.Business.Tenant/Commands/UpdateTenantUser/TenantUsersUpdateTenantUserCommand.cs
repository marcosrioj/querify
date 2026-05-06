using Querify.Models.Tenant.Enums;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenantUser;

public sealed class TenantUsersUpdateTenantUserCommand : IRequest
{
    public required Guid TenantId { get; set; }
    public required Guid Id { get; set; }
    public required TenantUserRoleType Role { get; set; }
}
