using Querify.Models.Tenant.Enums;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Commands.CreateTenantUser;

public sealed class TenantUsersCreateTenantUserCommand : IRequest<Guid>
{
    public required Guid TenantId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required TenantUserRoleType Role { get; set; }
}
