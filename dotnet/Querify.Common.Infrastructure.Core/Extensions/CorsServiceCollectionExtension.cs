using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Common.Infrastructure.Core.Extensions;

public static class CorsServiceCollectionExtension
{
    private static WebSocketOptions? _webSocketOptions;

    public static void AddCustomCors(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        var corsOptions = configuration.GetRequiredSection(CorsOptions.Name).Get<CorsOptions>();

        if (corsOptions == null)
        {
            throw new ApiErrorException(
                "Cors Options Not Found",
                errorCode: (int)HttpStatusCode.InternalServerError);
        }

        var extraAllowedCors = corsOptions.AllowedOrigins.Split(';');
        var corsUrls = new List<string>();
        corsUrls.AddRange(extraAllowedCors);

        // Add service and create Policy with options
        services.AddCors(options =>
        {
            if (corsOptions.AllowAnyOrigins)
            {
                options.AddPolicy("CorsPolicy",
                    b => b
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Content-Disposition",
                            "content-filename") // Allow headers on client side  https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Expose-Headers
                        .SetIsOriginAllowed(_ => true) //WorkAround to allow any
                        .AllowCredentials());
            }
            else
            {
                options.AddPolicy("CorsPolicy",
                    b => b
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithExposedHeaders("Content-Disposition",
                            "content-filename") // Allow headers on client side  https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Access-Control-Expose-Headers
                        .WithOrigins(corsUrls.ToArray())
                        .AllowCredentials()
                );

                //WebSocket CORS
                if (!corsOptions.EnableWebSocketCors)
                {
                    return;
                }

                _webSocketOptions = new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(120) };
                _webSocketOptions.AllowedOrigins.Clear();
                foreach (var item in corsUrls)
                {
                    _webSocketOptions.AllowedOrigins.Add(item);
                }
            }
        });
    }

    public static void UseCustomCors(this IApplicationBuilder app, ConfigurationManager configuration)
    {
        var corsOptions = configuration.GetRequiredSection(CorsOptions.Name).Get<CorsOptions>();

        if (corsOptions == null)
        {
            throw new ApiErrorException(
                "Cors Options Not Found",
                errorCode: (int)HttpStatusCode.InternalServerError);
        }

        if (corsOptions.EnableWebSocketCors && _webSocketOptions != null)
        {
            app.UseWebSockets(_webSocketOptions);
        }

        app.UseCors("CorsPolicy");
    }
}