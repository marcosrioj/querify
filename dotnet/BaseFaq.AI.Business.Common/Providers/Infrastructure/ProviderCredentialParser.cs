using BaseFaq.AI.Business.Common.Providers.Models;

namespace BaseFaq.AI.Business.Common.Providers.Infrastructure;

internal static class ProviderCredentialParser
{
    public static AiProviderCredential Parse(string? rawCredential)
    {
        if (string.IsNullOrWhiteSpace(rawCredential))
        {
            throw new InvalidOperationException("AI provider credential is not configured.");
        }

        var trimmedCredential = rawCredential.Trim();
        var parts = trimmedCredential.Split('|');

        if (parts.Length >= 2 && Uri.TryCreate(parts[0].Trim(), UriKind.Absolute, out var endpoint))
        {
            var apiKey = parts[1].Trim();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("AI provider API key is missing in credential payload.");
            }

            var deployment = parts.Length > 2 ? NullIfWhiteSpace(parts[2]) : null;
            var apiVersion = parts.Length > 3 ? NullIfWhiteSpace(parts[3]) : null;
            return new AiProviderCredential(apiKey, endpoint, deployment, apiVersion);
        }

        return new AiProviderCredential(trimmedCredential, null, null, null);
    }

    private static string? NullIfWhiteSpace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}