using System.Globalization;
using System.Text;
using BaseFaq.QnA.Common.Persistence.QnADb.DbContext;
using Microsoft.EntityFrameworkCore;
using QnASpace = BaseFaq.QnA.Common.Persistence.QnADb.Entities.Space;

namespace BaseFaq.QnA.Portal.Business.Space.Helpers;

internal static class SpaceSlugHelper
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
                GenerateSlug(requestedSlug),
                name,
                cancellationToken);

        if (!string.IsNullOrWhiteSpace(currentSlug))
            return currentSlug.Trim();

        return await EnsureUniqueSlugAsync(
            dbContext,
            tenantId,
            spaceId,
            GenerateSlug(name),
            name,
            cancellationToken);
    }

    public static string GenerateSlug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value
            .Trim()
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();
        var lastWasSeparator = false;

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category is UnicodeCategory.NonSpacingMark)
                continue;

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                lastWasSeparator = false;
                continue;
            }

            if (builder.Length == 0 || lastWasSeparator)
                continue;

            builder.Append('-');
            lastWasSeparator = true;
        }

        return TrimToMaxLength(builder.ToString().Trim('-'));
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
            baseSlug = GenerateSlug(name);

        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = TrimToMaxLength($"space-{Guid.NewGuid():N}");

        var candidate = baseSlug;
        var counter = 2;

        while (await dbContext.Spaces.AnyAsync(
                   space =>
                       space.TenantId == tenantId &&
                       (!spaceId.HasValue || space.Id != spaceId.Value) &&
                       space.Slug == candidate,
                   cancellationToken))
        {
            candidate = WithSuffix(baseSlug, counter);
            counter++;
        }

        return candidate;
    }

    private static string WithSuffix(string baseSlug, int counter)
    {
        var suffix = $"-{counter}";
        var maxBaseLength = QnASpace.MaxSlugLength - suffix.Length;
        var trimmedBase = baseSlug.Length > maxBaseLength
            ? baseSlug[..maxBaseLength].Trim('-')
            : baseSlug.Trim('-');

        if (string.IsNullOrWhiteSpace(trimmedBase))
            trimmedBase = "space";

        return $"{trimmedBase}{suffix}";
    }

    private static string TrimToMaxLength(string value)
    {
        if (value.Length <= QnASpace.MaxSlugLength)
            return value;

        return value[..QnASpace.MaxSlugLength].Trim('-');
    }
}
