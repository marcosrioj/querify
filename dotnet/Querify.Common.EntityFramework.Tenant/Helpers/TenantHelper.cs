using System.Globalization;
using System.Text;

namespace Querify.Common.EntityFramework.Tenant.Helpers;

public static class TenantHelper
{
    public static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var normalized = name
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var category = Char.GetUnicodeCategory(c);

            if (category != UnicodeCategory.NonSpacingMark &&
                char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
        }

        return sb.Length >= 1
            ? sb.ToString()
            : string.Empty;
    }
}