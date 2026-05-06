using System.Text.Json;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Microsoft.AspNetCore.Http;

namespace Querify.Common.Infrastructure.ApiErrorHandling.Middleware;

public class ApiErrorHandlingMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiErrorException apiError)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = apiError.ErrorCode;

            await context.Response.WriteAsync(JsonSerializer.Serialize(apiError.GetError()));
        }
        catch (ApiErrorConfirmationException apiError)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = apiError.ErrorCode;

            await context.Response.WriteAsync(apiError.Message);
        }
    }
}