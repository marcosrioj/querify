using System.Text;

namespace BaseFaq.AI.Business.Common.Providers.Infrastructure;

internal static class ProviderEndpointBuilder
{
    public static Uri Combine(Uri baseUri, string relativePath)
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var normalizedBase = baseUri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
            ? baseUri.AbsoluteUri
            : $"{baseUri.AbsoluteUri}/";

        var trimmedPath = relativePath.TrimStart('/');
        return new Uri(new Uri(normalizedBase), trimmedPath);
    }

    public static Uri CombineWithQuery(Uri baseUri, string relativePath, IReadOnlyDictionary<string, string?> query)
    {
        var endpoint = Combine(baseUri, relativePath);

        if (query.Count == 0)
        {
            return endpoint;
        }

        var builder = new StringBuilder();
        var isFirst = true;

        foreach (var pair in query)
        {
            if (string.IsNullOrWhiteSpace(pair.Key) || string.IsNullOrWhiteSpace(pair.Value))
            {
                continue;
            }

            builder.Append(isFirst ? '?' : '&');
            builder.Append(Uri.EscapeDataString(pair.Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(pair.Value));
            isFirst = false;
        }

        var querySuffix = builder.ToString();
        if (string.IsNullOrWhiteSpace(querySuffix))
        {
            return endpoint;
        }

        return new Uri($"{endpoint}{querySuffix}");
    }
}