using Querify.Models.Tenant.Enums;

namespace Querify.Models.Tenant.Dtos.TenantUser;

public class TenantUserUpdateRequestDto
{
    public required Guid TenantId { get; set; }
    public required TenantUserRoleType Role { get; set; }
}
