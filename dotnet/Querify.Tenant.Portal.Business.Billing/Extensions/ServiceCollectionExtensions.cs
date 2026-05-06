using Querify.Tenant.Portal.Business.Billing.Abstractions;
using Querify.Tenant.Portal.Business.Billing.Queries.GetBillingSummary;
using Querify.Tenant.Portal.Business.Billing.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Tenant.Portal.Business.Billing.Extensions;

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
