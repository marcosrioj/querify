using BaseFaq.Tenant.BackOffice.Business.Tenant.Abstractions;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenant;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Service;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenantBusiness(this IServiceCollection services)
    {
        services.AddScoped<ITenantConnectionService, TenantConnectionService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITenantUserService, TenantUserService>();
        services.AddMediatR(config =>
            config.RegisterServicesFromAssemblyContaining<TenantsCreateTenantCommandHandler>());

        return services;
    }
}
