using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using BaseFaq.Tenant.Portal.Business.Tenant.Commands.CreateOrUpdateTenants;
using BaseFaq.Tenant.Portal.Business.Tenant.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenantBusiness(this IServiceCollection services)
    {
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITenantAiProviderService, TenantAiProviderService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<TenantsCreateOrUpdateTenantsCommandHandler>());

        return services;
    }
}