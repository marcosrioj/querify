using BaseFaq.Tenant.Public.Business.Billing.Extensions;

namespace BaseFaq.Tenant.Public.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBillingBusiness(configuration);
    }
}
