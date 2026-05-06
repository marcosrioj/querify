using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Middleware;
using Microsoft.AspNetCore.Http;

namespace Querify.Common.Infrastructure.Core.Extensions;

public static class HttpContextTenantHeaderExtensions
{
    public static Guid GetTenantIdFromHeader(this HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.TryGetValue(TenantResolutionMiddleware.TenantHeaderName, out var headerValues))
        {
            throw new ApiErrorException(
                $"Missing required header '{TenantResolutionMiddleware.TenantHeaderName}'.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        var rawTenantId = headerValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(rawTenantId) || !Guid.TryParse(rawTenantId, out var tenantId))
        {
            throw new ApiErrorException(
                $"Header '{TenantResolutionMiddleware.TenantHeaderName}' must be a valid GUID.",
                errorCode: (int)HttpStatusCode.BadRequest);
        }

        return tenantId;
    }
}
