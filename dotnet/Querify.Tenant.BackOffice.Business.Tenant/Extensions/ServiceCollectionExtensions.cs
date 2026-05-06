using Querify.Tenant.BackOffice.Business.Tenant.Abstractions;
using Querify.Tenant.BackOffice.Business.Tenant.Commands.CreateTenant;
using Querify.Tenant.BackOffice.Business.Tenant.Service;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Tenant.BackOffice.Business.Tenant.Extensions;

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
