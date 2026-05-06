using Querify.Models.Tenant.Enums;
using MediatR;

namespace Querify.Tenant.Portal.Business.Tenant.Commands.AddTenantMember;

public sealed class TenantUsersAddTenantMemberCommand : IRequest<Guid>
{
    public required Guid TenantId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required TenantUserRoleType Role { get; set; }
}
