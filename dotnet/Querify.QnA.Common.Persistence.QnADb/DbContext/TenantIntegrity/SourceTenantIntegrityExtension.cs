using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
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

            if (string.IsNullOrWhiteSpace(source.Checksum))
                throw new ApiErrorException(
                    $"Source '{source.Id}' must include a non-empty checksum.",
                    (int)HttpStatusCode.BadRequest);

            if (source.Checksum.Length > Source.MaxChecksumLength)
                throw new ApiErrorException(
                    $"Source '{source.Id}' checksum exceeds {Source.MaxChecksumLength} characters.",
                    (int)HttpStatusCode.BadRequest);

        }
    }
}
