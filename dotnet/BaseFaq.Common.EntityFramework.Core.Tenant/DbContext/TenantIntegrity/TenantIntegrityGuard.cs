namespace BaseFaq.Common.EntityFramework.Core.Tenant.DbContext.TenantIntegrity;

public static class TenantIntegrityGuard
{
    public static void EnsureTenantMatch(Guid expectedTenantId, Guid actualTenantId, string relationshipName)
    {
        if (actualTenantId != expectedTenantId)
            throw new InvalidOperationException(
                $"Cross-tenant relationship detected for '{relationshipName}'. Expected tenant '{expectedTenantId}' but found '{actualTenantId}'.");
    }
}
