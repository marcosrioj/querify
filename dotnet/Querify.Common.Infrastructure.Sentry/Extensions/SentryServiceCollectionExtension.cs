using System.Text.Json;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Querify.Common.Infrastructure.Sentry.Extensions;

public static class SentryServiceCollectionExtension
{
    public static void AddConfiguredSentry(this IWebHostBuilder webBuilder, IWebHostEnvironment environment)
    {
        if (!environment.IsProduction())
        {
            return;
        }

        string release;

        try
        {
            var json = File.ReadAllText("build_info.json");

            var jsonObject = JsonDocument.Parse(json).RootElement;

            release = jsonObject.GetProperty("build_version").GetString() ?? "unknown";
        }
        catch (Exception)
        {
            release = "unknown";
        }

        webBuilder.UseSentry((_, options) =>
        {
            options.Release = release;
            options.AutoRegisterTracing = false;
            options.TracesSampleRate ??= 1.0;
            options.ProfilesSampleRate ??= 1.0;

            options.AddIntegration(new ProfilingIntegration(
                TimeSpan.FromMilliseconds(500)
            ));

            options.AddExceptionFilterForType<ApiErrorException>();
            options.AddExceptionFilterForType<ApiErrorConfirmationException>();
        });
    }

    public static void UseConfiguredSentry(this IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        if (env.IsProduction())
        {
            app.UseSentryTracing();
        }
    }
}
