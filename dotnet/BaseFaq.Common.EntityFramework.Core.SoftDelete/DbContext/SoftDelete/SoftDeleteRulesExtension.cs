using BaseFaq.Common.EntityFramework.Core.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.Common.EntityFramework.Core.SoftDelete.DbContext.SoftDelete;

public static class SoftDeleteRulesExtension
{
    public static void ApplySoftDeleteRules(this Microsoft.EntityFrameworkCore.DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State != EntityState.Deleted)
            {
                continue;
            }

            entry.Entity.IsDeleted = true;
            entry.State = EntityState.Modified;
            entry.Property(nameof(ISoftDelete.IsDeleted)).IsModified = true;
        }
    }
}
