using BaseFaq.Tenant.Portal.Business.AiProvider.Extensions;
using BaseFaq.Tenant.Portal.Business.Billing.Extensions;
using BaseFaq.Tenant.Portal.Business.Tenant.Extensions;
using BaseFaq.Tenant.Portal.Business.User.Extensions;

namespace BaseFaq.Tenant.Portal.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAiProviderBusiness();
        services.AddBillingBusiness();
        services.AddTenantBusiness();
        services.AddUserBusiness();
    }
}
