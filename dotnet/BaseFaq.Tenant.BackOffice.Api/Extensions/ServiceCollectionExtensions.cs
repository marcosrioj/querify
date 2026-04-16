using BaseFaq.Tenant.BackOffice.Business.Billing.Extensions;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Extensions;
using BaseFaq.Tenant.BackOffice.Business.User.Extensions;

namespace BaseFaq.Tenant.BackOffice.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBillingBusiness();
        services.AddTenantBusiness();
        services.AddUserBusiness();
    }
}
