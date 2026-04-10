using BaseFaq.Tenant.BackOffice.Business.Billing.Abstractions;
using BaseFaq.Tenant.BackOffice.Business.Billing.Commands.RequeueBillingWebhookInbox;
using BaseFaq.Tenant.BackOffice.Business.Billing.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Tenant.BackOffice.Business.Billing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBillingBusiness(this IServiceCollection services)
    {
        services.AddScoped<IBillingService, BillingService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<RequeueBillingWebhookInboxCommandHandler>());

        return services;
    }
}
