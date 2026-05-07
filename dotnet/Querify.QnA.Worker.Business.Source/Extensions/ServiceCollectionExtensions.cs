using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Querify.QnA.Common.Domain.Options;
using Querify.QnA.Worker.Business.Source.Abstractions;
using Querify.QnA.Worker.Business.Source.Commands.VerifyUploadedSource;
using Querify.QnA.Worker.Business.Source.HostedServices;
using Querify.QnA.Worker.Business.Source.Options;
using Querify.QnA.Worker.Business.Source.Services;

namespace Querify.QnA.Worker.Business.Source.Extensions;

public static class ServiceCollectionExtensions
{
    private const string NoopThreatScanningMode = "Noop";

    public static IServiceCollection AddSourceWorker(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddOptions<SourceUploadOptions>()
            .BindConfiguration(SourceUploadOptions.SectionName)
            .Validate(options => options.MaxUploadBytes > 0, "Source upload max size must be greater than zero.")
            .Validate(options => options.PendingExpirationHours > 0,
                "Source upload pending expiration hours must be greater than zero.")
            .Validate(options => options.AllowedContentTypes.Length > 0,
                "At least one source upload content type must be allowed.")
            .ValidateOnStart();

        services.AddOptions<SourceUploadedOutboxProcessingOptions>()
            .BindConfiguration(SourceUploadedOutboxProcessingOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<PendingSourceUploadExpiryOptions>()
            .BindConfiguration(PendingSourceUploadExpiryOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<VerifyUploadedSourceCommandHandler>());

        services.AddScoped<UploadContentInspector>();
        services.AddScoped<ISourceUploadedOutboxProcessor, SourceUploadedOutboxProcessor>();
        services.AddHostedService<SourceUploadedOutboxPublisherHostedService>();
        services.AddHostedService<PendingSourceUploadExpiryHostedService>();

        ConfigureThreatScanner(services, configuration, environment);

        return services;
    }

    private static void ConfigureThreatScanner(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var options = configuration.GetSection(SourceUploadOptions.SectionName).Get<SourceUploadOptions>() ?? new();
        var mode = options.ThreatScanningMode;

        if (environment.IsDevelopment() &&
            string.Equals(mode, NoopThreatScanningMode, StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IUploadThreatScanner, NoopUploadThreatScanner>();
            return;
        }

        if (string.Equals(mode, NoopThreatScanningMode, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "SourceUpload:ThreatScanningMode=Noop is allowed only in Development.");
        }

        throw new InvalidOperationException(
            "A production upload threat scanner is required. Configure a supported SourceUpload:ThreatScanningMode before running the QnA worker.");
    }
}
