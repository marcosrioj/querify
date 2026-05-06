using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Querify.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

internal static class SourceTenantIntegrityExtension
{
    internal static void EnsureSourceTenantIntegrity(this QnADbContext dbContext)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<Source>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var source = entry.Entity;

            if (source.Visibility is not VisibilityScope.Public) continue;

            if (source.Kind == SourceKind.InternalNote)
                throw new ApiErrorException(
                    $"Source '{source.Id}' cannot expose internal notes publicly.",
                    (int)HttpStatusCode.UnprocessableEntity);

            if (source.LastVerifiedAtUtc is null)
                throw new ApiErrorException(
                    $"Source '{source.Id}' must be verified before public exposure.",
                    (int)HttpStatusCode.UnprocessableEntity);
        }
    }
}
