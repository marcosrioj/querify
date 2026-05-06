using Querify.Tenant.BackOffice.Business.Billing.Abstractions;
using Querify.Tenant.BackOffice.Business.Billing.Commands.RequeueBillingWebhookInbox;
using Querify.Tenant.BackOffice.Business.Billing.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Tenant.BackOffice.Business.Billing.Extensions;

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
