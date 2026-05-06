using Querify.Tenant.Public.Business.Billing.Abstractions;
using Querify.Tenant.Public.Business.Billing.Commands.IngestStripeWebhook;
using Querify.Tenant.Public.Business.Billing.Options;
using Querify.Tenant.Public.Business.Billing.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Tenant.Public.Business.Billing.Extensions;

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
