using Querify.Tenant.Portal.Business.Billing.Extensions;
using Querify.Tenant.Portal.Business.Tenant.Extensions;
using Querify.Tenant.Portal.Business.User.Extensions;

namespace Querify.Tenant.Portal.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBillingBusiness();
        services.AddTenantBusiness();
        services.AddUserBusiness();
    }
}
