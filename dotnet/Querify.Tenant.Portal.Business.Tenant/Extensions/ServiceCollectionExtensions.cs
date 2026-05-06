using Querify.Tenant.Portal.Business.Tenant.Abstractions;
using Querify.Tenant.Portal.Business.Tenant.Commands.CreateOrUpdateTenants;
using Querify.Tenant.Portal.Business.Tenant.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Tenant.Portal.Business.Tenant.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenantBusiness(this IServiceCollection services)
    {
        services.AddScoped<ITenantPortalAccessService, TenantPortalAccessService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITenantUserService, TenantUserService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<TenantsCreateOrUpdateTenantsCommandHandler>());

        return services;
    }
}
