using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Models.Tenant.Dtos.TenantUser;

public class TenantUserUpdateRequestDto
{
    public required TenantUserRoleType Role { get; set; }
}
