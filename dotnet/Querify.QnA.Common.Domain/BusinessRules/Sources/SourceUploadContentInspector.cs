using System.Text;

namespace Querify.QnA.Common.Domain.BusinessRules.Sources;

public static class SourceUploadContentInspector
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    public static bool IsAllowed(
        string? contentType,
        ReadOnlySpan<byte> prefix,
        IEnumerable<string>? configuredAllowedContentTypes = null)
    {
        var normalizedContentType = SourceRules.NormalizeContentType(contentType);
        if (!SourceRules.IsUploadContentTypeAllowed(normalizedContentType, configuredAllowedContentTypes))
        {
            return false;
        }

        return normalizedContentType switch
        {
            "application/pdf" => HasPrefix(prefix, "%PDF-"u8),
            "image/png" => HasPrefix(prefix, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
            "image/jpeg" => prefix.Length >= 3 && prefix[0] == 0xFF && prefix[1] == 0xD8 && prefix[2] == 0xFF,
            "video/mp4" => prefix.Length >= 12 &&
                           prefix[4] == 0x66 &&
                           prefix[5] == 0x74 &&
                           prefix[6] == 0x79 &&
                           prefix[7] == 0x70,
            "text/plain" or "text/markdown" => LooksLikeText(prefix),
            _ => false
        };
    }

    private static bool HasPrefix(ReadOnlySpan<byte> value, ReadOnlySpan<byte> prefix)
    {
        return value.Length >= prefix.Length && value[..prefix.Length].SequenceEqual(prefix);
    }

    private static bool LooksLikeText(ReadOnlySpan<byte> prefix)
    {
        if (prefix.Contains((byte)0))
        {
            return false;
        }

        try
        {
            StrictUtf8.GetString(prefix);
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }
}
