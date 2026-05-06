using Querify.Tenant.BackOffice.Business.Billing.Extensions;
using Querify.Tenant.BackOffice.Business.Tenant.Extensions;
using Querify.Tenant.BackOffice.Business.User.Extensions;

namespace Querify.Tenant.BackOffice.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddFeatures(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBillingBusiness();
        services.AddTenantBusiness();
        services.AddUserBusiness();
    }
}
