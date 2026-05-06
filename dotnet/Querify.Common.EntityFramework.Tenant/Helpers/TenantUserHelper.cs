using Querify.Common.EntityFramework.Tenant.Entities;
using Querify.Models.Tenant.Enums;

namespace Querify.Common.EntityFramework.Tenant.Helpers;

public static class TenantUserHelper
{
    public static Guid? GetOwnerUserId(IEnumerable<TenantUser> tenantUsers)
    {
        return tenantUsers
            .Where(tenantUser => !tenantUser.IsDeleted && tenantUser.Role == TenantUserRoleType.Owner)
            .Select(tenantUser => (Guid?)tenantUser.UserId)
            .FirstOrDefault();
    }

    public static void SetOwner(ICollection<TenantUser> tenantUsers, Guid tenantId, Guid ownerUserId)
    {
        TenantUser? ownerMembership = null;

        foreach (var tenantUser in tenantUsers.Where(tenantUser => !tenantUser.IsDeleted))
        {
            if (tenantUser.UserId == ownerUserId)
            {
                tenantUser.Role = TenantUserRoleType.Owner;
                ownerMembership = tenantUser;
                continue;
            }

            if (tenantUser.Role == TenantUserRoleType.Owner)
            {
                tenantUser.Role = TenantUserRoleType.Member;
            }
        }

        if (ownerMembership is not null)
        {
            return;
        }

        tenantUsers.Add(new TenantUser
        {
            TenantId = tenantId,
            UserId = ownerUserId,
            Role = TenantUserRoleType.Owner
        });
    }
}
