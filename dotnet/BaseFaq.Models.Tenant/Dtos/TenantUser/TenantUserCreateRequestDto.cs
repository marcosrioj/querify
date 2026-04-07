using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Models.Tenant.Dtos.TenantUser;

public class TenantUserCreateRequestDto
{
    public required string Email { get; set; }
    public required TenantUserRoleType Role { get; set; }
}
