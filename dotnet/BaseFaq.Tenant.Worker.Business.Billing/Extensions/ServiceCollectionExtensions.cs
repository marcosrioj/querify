using BaseFaq.Tenant.Worker.Business.Billing.Abstractions;
using BaseFaq.Tenant.Worker.Business.Billing.Commands.DispatchBillingWebhookInbox;
using BaseFaq.Tenant.Worker.Business.Billing.HostedServices;
using BaseFaq.Tenant.Worker.Business.Billing.Options;
using BaseFaq.Tenant.Worker.Business.Billing.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Tenant.Worker.Business.Billing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBillingBusiness(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<BillingProcessingOptions>()
            .Bind(configuration.GetSection(BillingProcessingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<DispatchBillingWebhookInboxCommandHandler>());

        services.AddScoped<IBillingWebhookInboxProcessor, BillingWebhookInboxProcessor>();
        services.AddHostedService<BillingWebhookInboxProcessorHostedService>();

        return services;
    }
}
