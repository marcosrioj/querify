using Querify.Models.Tenant.Enums;

namespace Querify.Models.Tenant.Dtos.TenantUser;

public class TenantUserCreateRequestDto
{
    public required Guid TenantId { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required TenantUserRoleType Role { get; set; }
}
