using Querify.Tenant.Worker.Business.Email.Abstractions;
using Querify.Tenant.Worker.Business.Email.Commands.SendEmailOutbox;
using Querify.Tenant.Worker.Business.Email.HostedServices;
using Querify.Tenant.Worker.Business.Email.Options;
using Querify.Tenant.Worker.Business.Email.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Tenant.Worker.Business.Email.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailBusiness(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<EmailProcessingOptions>()
            .Bind(configuration.GetSection(EmailProcessingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<EmailDeliveryOptions>()
            .Bind(configuration.GetSection(EmailDeliveryOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<SendEmailOutboxCommandHandler>());

        services.AddScoped<IEmailOutboxProcessorService, EmailOutboxProcessorService>();
        services.AddHostedService<EmailOutboxProcessorHostedService>();

        return services;
    }
}
