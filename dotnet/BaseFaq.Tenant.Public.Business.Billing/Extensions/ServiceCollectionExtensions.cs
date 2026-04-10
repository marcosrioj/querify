using BaseFaq.Tenant.Public.Business.Billing.Abstractions;
using BaseFaq.Tenant.Public.Business.Billing.Commands.IngestStripeWebhook;
using BaseFaq.Tenant.Public.Business.Billing.Options;
using BaseFaq.Tenant.Public.Business.Billing.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Tenant.Public.Business.Billing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBillingBusiness(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<StripeWebhookOptions>()
            .Bind(configuration.GetSection(StripeWebhookOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IBillingWebhookService, BillingWebhookService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<IngestStripeWebhookCommand>());

        return services;
    }
}
