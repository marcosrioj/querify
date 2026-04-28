using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BaseFaq.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

internal static class SourceTenantIntegrityExtension
{
    internal static void EnsureSourceTenantIntegrity(this QnADbContext dbContext)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<Source>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var source = entry.Entity;

            if (!source.Visibility.IsPubliclyVisible())
            {
                if (source.AllowsPublicCitation || source.AllowsPublicExcerpt)
                    throw new InvalidOperationException(
                        $"Source '{source.Id}' cannot allow public citation or excerpt reuse while not publicly visible.");

                continue;
            }

            if (source.Kind == SourceKind.InternalNote)
                throw new InvalidOperationException(
                    $"Source '{source.Id}' cannot expose internal notes publicly.");

            if (source.LastVerifiedAtUtc is null)
                throw new InvalidOperationException(
                    $"Source '{source.Id}' must be verified before public exposure.");
        }
    }
}
