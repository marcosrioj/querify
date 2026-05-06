using Querify.QnA.Common.Domain.BusinessRules.Spaces;
using Querify.QnA.Common.Persistence.QnADb.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Querify.QnA.Portal.Business.Space.Commands;

internal static class SpaceSlugResolver
{
    public static async Task<string> ResolveSlugAsync(
        QnADbContext dbContext,
        Guid tenantId,
        Guid? spaceId,
        string? requestedSlug,
        string name,
        string? currentSlug,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(requestedSlug))
            return await EnsureUniqueSlugAsync(
                dbContext,
                tenantId,
                spaceId,
                SpaceSlugRules.GenerateSlug(requestedSlug),
                name,
                cancellationToken);

        if (!string.IsNullOrWhiteSpace(currentSlug))
            return currentSlug.Trim();

        return await EnsureUniqueSlugAsync(
            dbContext,
            tenantId,
            spaceId,
            SpaceSlugRules.GenerateSlug(name),
            name,
            cancellationToken);
    }

    private static async Task<string> EnsureUniqueSlugAsync(
        QnADbContext dbContext,
        Guid tenantId,
        Guid? spaceId,
        string baseSlug,
        string name,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = SpaceSlugRules.GenerateSlug(name);

        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = SpaceSlugRules.GenerateFallbackSlug();

        var candidate = baseSlug;
        var counter = 2;

        while (await dbContext.Spaces.AnyAsync(
                   space =>
                       space.TenantId == tenantId &&
                       (!spaceId.HasValue || space.Id != spaceId.Value) &&
                       space.Slug == candidate,
                   cancellationToken))
        {
            candidate = SpaceSlugRules.WithSuffix(baseSlug, counter);
            counter++;
        }

        return candidate;
    }
}
