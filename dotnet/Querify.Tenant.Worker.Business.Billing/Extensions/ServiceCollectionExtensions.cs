using Querify.Tenant.Worker.Business.Billing.Abstractions;
using Querify.Tenant.Worker.Business.Billing.Commands.DispatchBillingWebhookInbox;
using Querify.Tenant.Worker.Business.Billing.EventHandlers;
using Querify.Tenant.Worker.Business.Billing.HostedServices;
using Querify.Tenant.Worker.Business.Billing.Options;
using Querify.Tenant.Worker.Business.Billing.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Tenant.Worker.Business.Billing.Extensions;

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
        services.AddOptions<StripeBillingOptions>()
            .Bind(configuration.GetSection(StripeBillingOptions.SectionName));

        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<DispatchBillingWebhookInboxCommandHandler>());

        services.AddScoped<IBillingWebhookInboxProcessor, BillingWebhookInboxProcessor>();
        services.AddScoped<IBillingProvider, StripeBillingProvider>();
        services.AddScoped<IBillingProviderResolver, BillingProviderResolver>();
        services.AddScoped<IBillingWebhookDispatcher, BillingWebhookDispatcher>();
        services.AddScoped<StripeWebhookEventMapper>();
        services.AddScoped<BillingTenantResolver>();
        services.AddScoped<BillingStateService>();
        services.AddScoped<IBillingWebhookEventHandler, UnknownBillingWebhookEventHandler>();
        services.AddScoped<IBillingWebhookEventHandler, CheckoutCompletedBillingWebhookEventHandler>();
        services.AddScoped<IBillingWebhookEventHandler, SubscriptionCreatedBillingWebhookEventHandler>();
        services.AddScoped<IBillingWebhookEventHandler, SubscriptionUpdatedBillingWebhookEventHandler>();
        services.AddScoped<IBillingWebhookEventHandler, SubscriptionCanceledBillingWebhookEventHandler>();
        services.AddScoped<IBillingWebhookEventHandler, InvoicePaidBillingWebhookEventHandler>();
        services.AddScoped<IBillingWebhookEventHandler, InvoicePaymentFailedBillingWebhookEventHandler>();
        services.AddHostedService<BillingWebhookInboxProcessorHostedService>();

        return services;
    }
}
