using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BaseFaq.QnA.Common.Persistence.QnADb.DbContext.TenantIntegrity;

internal static class SpaceTenantIntegrityExtension
{
    internal static void EnsureSpaceTenantIntegrity(this QnADbContext dbContext)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries<Space>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified))
        {
            var space = entry.Entity;

            if (space.Visibility is VisibilityScope.Public &&
                space.Status is not SpaceStatus.Active)
                throw new ApiErrorException(
                    $"Space '{space.Id}' cannot be public while in status '{space.Status}'.",
                    (int)HttpStatusCode.UnprocessableEntity);
        }
    }
}
