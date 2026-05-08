using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.QnA.Dtos.Source;
using Microsoft.Extensions.Caching.Memory;

namespace Querify.QnA.Portal.Business.Source.Queries.InspectExternalUrl;

public sealed class SourceExternalUrlInspectionCoordinator(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache)
{
    public const string HttpClientName = "Querify.QnA.Source.ExternalUrlInspection";
    private const int MaxConcurrentInspections = 16;
    private const int MaxRedirects = 5;
    private const int MaxTrackedInspections = 512;

    private readonly SemaphoreSlim concurrencyGate = new(MaxConcurrentInspections, MaxConcurrentInspections);
    private readonly ConcurrentDictionary<string, Lazy<Task<SourceExternalUrlInspectionDto>>> inFlightInspections = new();

    public async Task<SourceExternalUrlInspectionDto> InspectAsync(Uri uri, CancellationToken cancellationToken)
    {
        var normalizedUri = NormalizeUri(uri);
        var cacheKey = BuildCacheKey(normalizedUri);

        if (cache.TryGetValue<SourceExternalUrlInspectionDto>(cacheKey, out var cached) && cached is not null)
            return cached;

        if (inFlightInspections.Count >= MaxTrackedInspections && !inFlightInspections.ContainsKey(cacheKey))
            return BuildSyntheticResponse(normalizedUri, HttpStatusCode.TooManyRequests, "Too Many Requests");

        var candidate = new Lazy<Task<SourceExternalUrlInspectionDto>>(
            () => InspectAndCacheAsync(normalizedUri, cacheKey),
            LazyThreadSafetyMode.ExecutionAndPublication);
        var inFlight = inFlightInspections.GetOrAdd(cacheKey, candidate);

        if (ReferenceEquals(candidate, inFlight) && inFlightInspections.Count > MaxTrackedInspections)
        {
            inFlightInspections.TryRemove(new KeyValuePair<string, Lazy<Task<SourceExternalUrlInspectionDto>>>(
                cacheKey,
                inFlight));
            return BuildSyntheticResponse(normalizedUri, HttpStatusCode.TooManyRequests, "Too Many Requests");
        }

        if (ReferenceEquals(candidate, inFlight))
            _ = inFlight.Value.ContinueWith(
                completed =>
                {
                    _ = completed.Exception;
                    return inFlightInspections.TryRemove(new KeyValuePair<string, Lazy<Task<SourceExternalUrlInspectionDto>>>(
                        cacheKey,
                        inFlight));
                },
                TaskScheduler.Default);

        return await inFlight.Value.WaitAsync(cancellationToken);
    }

    private async Task<SourceExternalUrlInspectionDto> InspectAndCacheAsync(Uri uri, string cacheKey)
    {
        await concurrencyGate.WaitAsync(CancellationToken.None);

        try
        {
            var result = await ExecuteInspectionAsync(uri);
            cache.Set(cacheKey, result, GetCacheDuration(result));
            return result;
        }
        finally
        {
            concurrencyGate.Release();
        }
    }

    private async Task<SourceExternalUrlInspectionDto> ExecuteInspectionAsync(Uri uri)
    {
        var client = httpClientFactory.CreateClient(HttpClientName);

        try
        {
            using var headResponse = await SendFollowingRedirectsAsync(client, uri, HttpMethod.Head);
            var response = headResponse;

            if (headResponse.StatusCode is HttpStatusCode.MethodNotAllowed or HttpStatusCode.NotImplemented)
            {
                headResponse.Dispose();
                using var getResponse = await SendFollowingRedirectsAsync(client, uri, HttpMethod.Get);
                response = getResponse;
                return BuildResponse(response);
            }

            return BuildResponse(response);
        }
        catch (TaskCanceledException)
        {
            return BuildSyntheticResponse(uri, HttpStatusCode.RequestTimeout, "Request Timeout");
        }
        catch (HttpRequestException)
        {
            return BuildNetworkFailureResponse(uri);
        }
        catch (SocketException)
        {
            return BuildNetworkFailureResponse(uri);
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

    private static SourceExternalUrlInspectionDto BuildNetworkFailureResponse(Uri uri)
    {
        return new SourceExternalUrlInspectionDto
        {
            IsReachable = false,
            FinalUrl = uri.ToString()
        };
    }

    private static SourceExternalUrlInspectionDto BuildSyntheticResponse(
        Uri uri,
        HttpStatusCode statusCode,
        string statusText)
    {
        return new SourceExternalUrlInspectionDto
        {
            IsReachable = false,
            Status = (int)statusCode,
            StatusText = statusText,
            FinalUrl = uri.ToString()
        };
    }

    private async Task<HttpResponseMessage> SendFollowingRedirectsAsync(
        HttpClient client,
        Uri uri,
        HttpMethod method)
    {
        var currentUri = uri;

        for (var redirectCount = 0; redirectCount <= MaxRedirects; redirectCount++)
        {
            await EnsureSafeExternalUriAsync(currentUri);

            var request = new HttpRequestMessage(method, currentUri);
            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

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

    private async Task EnsureSafeExternalUriAsync(Uri uri)
    {
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            throw new ApiErrorException("External URL must use HTTP or HTTPS.", (int)HttpStatusCode.UnprocessableEntity);

        if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            throw new ApiErrorException("External URL must resolve to a public host.",
                (int)HttpStatusCode.UnprocessableEntity);

        var addresses = await GetHostAddressesAsync(uri.Host, CancellationToken.None);
        if (addresses.Length == 0 || addresses.Any(IsUnsafeAddress))
            throw new ApiErrorException("External URL must resolve to a public host.",
                (int)HttpStatusCode.UnprocessableEntity);
    }

    private static Task<IPAddress[]> GetHostAddressesAsync(string host, CancellationToken cancellationToken)
    {
        return Dns.GetHostAddressesAsync(host, cancellationToken);
    }

    private static bool IsUnsafeAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
            return IsUnsafeAddress(address.MapToIPv4());

        if (IPAddress.IsLoopback(address))
            return true;

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var bytes = address.GetAddressBytes();
            return bytes[0] is 0 or 10 or 127 ||
                   bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127 ||
                   bytes[0] == 169 && bytes[1] == 254 ||
                   bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31 ||
                   bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 0 ||
                   bytes[0] == 192 && bytes[1] == 0 && bytes[2] == 2 ||
                   bytes[0] == 192 && bytes[1] == 168 ||
                   bytes[0] == 198 && bytes[1] is 18 or 19 ||
                   bytes[0] == 198 && bytes[1] == 51 && bytes[2] == 100 ||
                   bytes[0] == 203 && bytes[1] == 0 && bytes[2] == 113 ||
                   bytes[0] >= 224;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            var bytes = address.GetAddressBytes();
            return bytes.All(value => value == 0) ||
                   address.IsIPv6LinkLocal ||
                   address.IsIPv6Multicast ||
                   address.IsIPv6SiteLocal ||
                   bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0x0D && bytes[3] == 0xB8 ||
                   bytes[0] is 0xFC or 0xFD;
        }

        return true;
    }

    private static Uri NormalizeUri(Uri uri)
    {
        var builder = new UriBuilder(uri)
        {
            Fragment = string.Empty,
            Host = uri.Host.ToLowerInvariant(),
            Scheme = uri.Scheme.ToLowerInvariant()
        };

        if (uri.IsDefaultPort)
            builder.Port = -1;

        return builder.Uri;
    }

    private static string BuildCacheKey(Uri uri)
    {
        return $"qna-source-external-url-inspection:{uri.AbsoluteUri}";
    }

    private static TimeSpan GetCacheDuration(SourceExternalUrlInspectionDto result)
    {
        if (result.IsReachable)
            return TimeSpan.FromMinutes(20);

        if (result.Status is >= 400 and < 500 &&
            result.Status is not (int)HttpStatusCode.RequestTimeout and not (int)HttpStatusCode.TooManyRequests)
            return TimeSpan.FromMinutes(10);

        return TimeSpan.FromMinutes(2);
    }
}
