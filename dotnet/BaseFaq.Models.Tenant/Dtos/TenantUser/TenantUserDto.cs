using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Models.Tenant.Dtos.TenantUser;

public class TenantUserDto
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required Guid UserId { get; set; }
    public required string GivenName { get; set; }
    public string? SurName { get; set; }
    public required string Email { get; set; }
    public required TenantUserRoleType Role { get; set; }
    public bool IsCurrentUser { get; set; }
}
