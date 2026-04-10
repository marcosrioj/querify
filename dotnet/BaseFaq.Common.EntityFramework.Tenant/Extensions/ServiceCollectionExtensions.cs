using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Tenant.Abstractions;
using BaseFaq.Common.EntityFramework.Tenant.Providers;
using BaseFaq.Common.EntityFramework.Tenant.Services;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Common.EntityFramework.Tenant.Extensions;

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
        services.AddScoped<ITenantAiProviderResolver, TenantAiProviderResolver>();
        services.AddScoped<ITenantEntitlementSynchronizer, TenantEntitlementSynchronizer>();

        return services;
    }
}
