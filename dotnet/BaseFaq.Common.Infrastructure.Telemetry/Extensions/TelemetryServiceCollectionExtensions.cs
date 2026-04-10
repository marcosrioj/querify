using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BaseFaq.Common.Infrastructure.Telemetry.Extensions;

public static class TelemetryServiceCollectionExtensions
{
    public static IServiceCollection AddTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        params string[] additionalActivitySources)
    {
        var serviceName = configuration["OpenTelemetry:ServiceName"] ?? "basefaq-service";
        var serviceVersion = typeof(TelemetryServiceCollectionExtensions).Assembly.GetName().Version?.ToString() ??
                             "unknown";
        var otlpEndpoint = configuration["OpenTelemetry:Otlp:Endpoint"];

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("deployment.environment.name", environment.EnvironmentName)
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddSource("MassTransit");

                foreach (var source in additionalActivitySources.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    tracing.AddSource(source);
                }

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    tracing.AddOtlpExporter(exporter => { exporter.Endpoint = new Uri(otlpEndpoint); });
                }
            });

        return services;
    }
}