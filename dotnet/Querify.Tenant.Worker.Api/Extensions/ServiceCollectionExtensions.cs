using Querify.Tenant.Worker.Business.Billing.Extensions;
using Querify.Tenant.Worker.Business.Email.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Tenant.Worker.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddTenantWorkerFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBillingBusiness(configuration);
        services.AddEmailBusiness(configuration);
    }
}
