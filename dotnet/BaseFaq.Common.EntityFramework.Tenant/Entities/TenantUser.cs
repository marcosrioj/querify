using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Common.EntityFramework.Tenant.Entities;

public class TenantUser : BaseEntity
{
    public required Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public required TenantUserRoleType Role { get; set; }
}
