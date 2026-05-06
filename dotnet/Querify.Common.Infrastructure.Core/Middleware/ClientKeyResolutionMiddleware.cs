using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Constants;
using Microsoft.AspNetCore.Http;

namespace Querify.Common.Infrastructure.Core.Middleware;

public sealed class ClientKeyResolutionMiddleware(RequestDelegate next)
{
    public const string ClientKeyHeaderName = "X-Client-Key";

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(ClientKeyHeaderName, out var headerValues))
        {
            throw new ApiErrorException(
                $"Missing required header '{ClientKeyHeaderName}'.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        var rawClientKey = headerValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(rawClientKey))
        {
            throw new ApiErrorException(
                $"Header '{ClientKeyHeaderName}' is required.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        context.Items[ClientKeyContextKeys.ClientKeyItemKey] = rawClientKey.Trim();

        await next(context);
    }
}