using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Models.Tenant.Dtos.TenantUser;

public class TenantUserUpdateRequestDto
{
    public required Guid TenantId { get; set; }
    public required TenantUserRoleType Role { get; set; }
}
