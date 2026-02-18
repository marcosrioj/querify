using BaseFaq.AI.Persistence.AiDb.Infrastructure;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BaseFaq.AI.Persistence.AiDb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAiDb(this IServiceCollection services, string? connectionString)
    {
        var migrationsAssembly = typeof(AiDbContext).Assembly.GetName().Name;

        services.AddHttpContextAccessor();
        services.TryAddScoped<ISessionService, AiDbSessionService>();
        services.TryAddScoped<ITenantConnectionStringProvider, AiTenantConnectionStringProvider>();

        services.AddDbContext<AiDbContext>(options =>
            options.UseNpgsql(connectionString,
                b => b.EnableRetryOnFailure().MigrationsAssembly(migrationsAssembly)));

        return services;
    }
}