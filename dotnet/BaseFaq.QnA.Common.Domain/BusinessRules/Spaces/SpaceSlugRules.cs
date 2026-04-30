using System.Globalization;
using System.Text;
using BaseFaq.QnA.Common.Domain.Entities;

namespace BaseFaq.QnA.Common.Domain.BusinessRules.Spaces;

public static class SpaceSlugRules
{
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

    public static string GenerateFallbackSlug()
    {
        return TrimToMaxLength($"space-{Guid.NewGuid():N}");
    }

    public static string WithSuffix(string baseSlug, int counter)
    {
        var suffix = $"-{counter}";
        var maxBaseLength = Space.MaxSlugLength - suffix.Length;
        var trimmedBase = baseSlug.Length > maxBaseLength
            ? baseSlug[..maxBaseLength].Trim('-')
            : baseSlug.Trim('-');

        if (string.IsNullOrWhiteSpace(trimmedBase))
            trimmedBase = "space";

        return $"{trimmedBase}{suffix}";
    }

    private static string TrimToMaxLength(string value)
    {
        if (value.Length <= Space.MaxSlugLength)
            return value;

        return value[..Space.MaxSlugLength].Trim('-');
    }
}
