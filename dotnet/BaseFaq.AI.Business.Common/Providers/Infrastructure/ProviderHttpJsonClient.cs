using System.Net.Http.Json;
using BaseFaq.AI.Business.Common.Utilities;

namespace BaseFaq.AI.Business.Common.Providers.Infrastructure;

public sealed class ProviderHttpJsonClient
{
    private const int MaxErrorBodyLength = 1500;
    private static readonly HttpClient SharedClient = new();

    public async Task<string> PostJsonAsync(
        Uri endpoint,
        object payload,
        Action<HttpRequestMessage> configure,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = JsonContent.Create(payload)
        };

        configure(request);

        using var response = await SharedClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return body;
        }

        var trimmedBody = TextBounds.Truncate(body, MaxErrorBodyLength);
        throw new InvalidOperationException(
            $"Provider request failed with status {(int)response.StatusCode}: {trimmedBody}");
    }
}