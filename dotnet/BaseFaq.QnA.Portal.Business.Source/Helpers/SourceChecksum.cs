using System.Security.Cryptography;
using System.Text;

namespace BaseFaq.QnA.Portal.Business.Source.Helpers;

internal static class SourceChecksum
{
    public static string FromLocator(string locator)
    {
        var normalizedLocator = locator.Trim();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedLocator));
        return $"sha256:{Convert.ToHexString(hash).ToLowerInvariant()}";
    }
}
