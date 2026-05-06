using Querify.Common.Infrastructure.ApiErrorHandling.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Querify.Common.Infrastructure.ApiErrorHandling.Extensions;

public static class ApiErrorHandlingServiceCollectionExtension
{
    public static void UseApiErrorHandlingMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ApiErrorHandlingMiddleware>();
    }
}