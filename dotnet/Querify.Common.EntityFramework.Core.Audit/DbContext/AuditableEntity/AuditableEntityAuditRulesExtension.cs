using Microsoft.EntityFrameworkCore;
using AuditableEntityBase = Querify.Common.EntityFramework.Core.Entities.AuditableEntity;

namespace Querify.Common.EntityFramework.Core.Audit.DbContext.AuditableEntity;

public static class AuditableEntityAuditRulesExtension
{
    public static void ApplyAuditRules(this Microsoft.EntityFrameworkCore.DbContext context, string? userId)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntityBase>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate ??= now;
                entry.Entity.UpdatedDate = now;

                if (string.IsNullOrWhiteSpace(entry.Entity.CreatedBy))
                {
                    entry.Entity.CreatedBy = userId;
                }

                entry.Entity.UpdatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(AuditableEntityBase.CreatedDate)).IsModified = false;
                entry.Property(nameof(AuditableEntityBase.CreatedBy)).IsModified = false;

                entry.Entity.UpdatedDate = now;
                entry.Entity.UpdatedBy = userId;

                if (entry.Entity.IsDeleted &&
                    entry.Property(nameof(AuditableEntityBase.IsDeleted)).IsModified)
                {
                    entry.Entity.DeletedDate = now;
                    entry.Entity.DeletedBy = userId;
                }
            }
        }
    }
}
