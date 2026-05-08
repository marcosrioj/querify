using System.Net;
using System.Net.Sockets;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.QnA.Dtos.Source;
using MediatR;

namespace Querify.QnA.Portal.Business.Source.Queries.InspectExternalUrl;

public sealed class SourcesInspectExternalUrlQueryHandler(IHttpClientFactory httpClientFactory)
    : IRequestHandler<SourcesInspectExternalUrlQuery, SourceExternalUrlInspectionDto>
{
    public const string HttpClientName = "Querify.QnA.Source.ExternalUrlInspection";
    private const int MaxRedirects = 5;

    public async Task<SourceExternalUrlInspectionDto> Handle(
        SourcesInspectExternalUrlQuery request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);

        if (!Uri.TryCreate(request.Request.Locator, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ApiErrorException("External URL must use HTTP or HTTPS.", (int)HttpStatusCode.UnprocessableEntity);

        var client = httpClientFactory.CreateClient(HttpClientName);

        try
        {
            using var headResponse = await SendFollowingRedirectsAsync(client, uri, HttpMethod.Head, cancellationToken);
            var response = headResponse;

            if (headResponse.StatusCode is HttpStatusCode.MethodNotAllowed or HttpStatusCode.NotImplemented)
            {
                headResponse.Dispose();
                using var getResponse = await SendFollowingRedirectsAsync(client, uri, HttpMethod.Get, cancellationToken);
                response = getResponse;
                return BuildResponse(response);
            }

            return BuildResponse(response);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new SourceExternalUrlInspectionDto
            {
                IsReachable = false,
                Status = (int)HttpStatusCode.RequestTimeout,
                StatusText = "Request Timeout",
                FinalUrl = uri.ToString()
            };
        }
        catch (HttpRequestException)
        {
            return new SourceExternalUrlInspectionDto
            {
                IsReachable = false,
                FinalUrl = uri.ToString()
            };
        }
        catch (SocketException)
        {
            return new SourceExternalUrlInspectionDto
            {
                IsReachable = false,
                FinalUrl = uri.ToString()
            };
        }
    }

    private static SourceExternalUrlInspectionDto BuildResponse(HttpResponseMessage response)
    {
        return new SourceExternalUrlInspectionDto
        {
            IsReachable = response.IsSuccessStatusCode,
            Status = (int)response.StatusCode,
            StatusText = response.ReasonPhrase,
            FinalUrl = response.RequestMessage?.RequestUri?.ToString(),
            ContentType = response.Content.Headers.ContentType?.MediaType,
            ContentLengthBytes = response.Content.Headers.ContentLength,
            LastModified = response.Content.Headers.LastModified?.ToString("R")
        };
    }

    private static async Task<HttpResponseMessage> SendFollowingRedirectsAsync(
        HttpClient client,
        Uri uri,
        HttpMethod method,
        CancellationToken cancellationToken)
    {
        var currentUri = uri;

        for (var redirectCount = 0; redirectCount <= MaxRedirects; redirectCount++)
        {
            await EnsureSafeExternalUriAsync(currentUri, cancellationToken);

            var request = new HttpRequestMessage(method, currentUri);
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!IsRedirect(response.StatusCode) || response.Headers.Location is null)
                return response;

            currentUri = response.Headers.Location.IsAbsoluteUri
                ? response.Headers.Location
                : new Uri(currentUri, response.Headers.Location);
            response.Dispose();
        }

        return new HttpResponseMessage((HttpStatusCode)508)
        {
            ReasonPhrase = "Loop Detected",
            RequestMessage = new HttpRequestMessage(method, currentUri)
        };
    }

    private static bool IsRedirect(HttpStatusCode statusCode)
    {
        var status = (int)statusCode;
        return status is 301 or 302 or 303 or 307 or 308;
    }

    private static async Task EnsureSafeExternalUriAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            throw new ApiErrorException("External URL must use HTTP or HTTPS.", (int)HttpStatusCode.UnprocessableEntity);

        if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            throw new ApiErrorException("External URL must resolve to a public host.",
                (int)HttpStatusCode.UnprocessableEntity);

        var addresses = await Dns.GetHostAddressesAsync(uri.Host, cancellationToken);
        if (addresses.Length == 0 || addresses.Any(IsUnsafeAddress))
            throw new ApiErrorException("External URL must resolve to a public host.",
                (int)HttpStatusCode.UnprocessableEntity);
    }

    private static bool IsUnsafeAddress(IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
            return true;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            return bytes[0] is 0 or 10 or 127 ||
                   bytes[0] == 169 && bytes[1] == 254 ||
                   bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31 ||
                   bytes[0] == 192 && bytes[1] == 168 ||
                   bytes[0] >= 224;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            var bytes = address.GetAddressBytes();
            return address.IsIPv6LinkLocal ||
                   address.IsIPv6Multicast ||
                   address.IsIPv6SiteLocal ||
                   bytes[0] is 0xFC or 0xFD;
        }

        return true;
    }
}
