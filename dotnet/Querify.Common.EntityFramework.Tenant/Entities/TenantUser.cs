using Querify.Common.EntityFramework.Core.Entities;
using Querify.Models.Tenant.Enums;

namespace Querify.Common.EntityFramework.Tenant.Entities;

public class TenantUser : BaseEntity
{
    public required Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public required Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public required TenantUserRoleType Role { get; set; }
}
