using System.Security.Cryptography;
using System.Text;

namespace Querify.QnA.Common.Domain.BusinessRules.Sources;

public static class SourceChecksum
{
    public static string FromLocator(string locator)
    {
        var normalizedLocator = locator.Trim();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedLocator));
        return $"sha256:{Convert.ToHexString(hash).ToLowerInvariant()}";
    }

    public static string? NormalizeOptional(string? checksum)
    {
        if (string.IsNullOrWhiteSpace(checksum))
        {
            return null;
        }

        var normalized = checksum.Trim().ToLowerInvariant();
        return normalized.StartsWith("sha256:", StringComparison.Ordinal)
            ? normalized
            : $"sha256:{normalized}";
    }

    public static bool IsLocatorChecksum(string checksum, string locator)
    {
        return string.Equals(
            NormalizeOptional(checksum),
            FromLocator(locator),
            StringComparison.Ordinal);
    }
}
