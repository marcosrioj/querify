using BaseFaq.Tenant.Portal.Business.Billing.Abstractions;
using BaseFaq.Tenant.Portal.Business.Billing.Queries.GetBillingSummary;
using BaseFaq.Tenant.Portal.Business.Billing.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Tenant.Portal.Business.Billing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBillingBusiness(this IServiceCollection services)
    {
        services.AddScoped<IBillingPortalService, BillingPortalService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<GetBillingSummaryQueryHandler>());

        return services;
    }
}
