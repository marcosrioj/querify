using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.EntityFramework.Tenant.Abstractions;
using Querify.Common.EntityFramework.Tenant.Providers;
using Querify.Common.EntityFramework.Tenant.Services;
using Querify.Common.Infrastructure.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Common.EntityFramework.Tenant.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenantDb(this IServiceCollection services, string? connectionString)
    {
        var migrationsAssembly = typeof(TenantDbContext).Assembly.GetName().Name;

        services.AddDbContext<TenantDbContext>(options =>
            options.UseNpgsql(connectionString,
                b => b.EnableRetryOnFailure().MigrationsAssembly(migrationsAssembly)));

        services.AddMemoryCache();

        services.AddScoped<ITenantConnectionStringProvider, TenantConnectionStringProvider>();
        services.AddScoped<IUserIdProvider, UserIdProvider>();
        services.AddScoped<IAllowedTenantProvider, AllowedTenantProvider>();
        services.AddScoped<ITenantClientKeyResolver, TenantClientKeyResolver>();
        services.AddScoped<ITenantEntitlementSynchronizer, TenantEntitlementSynchronizer>();

        return services;
    }
}
