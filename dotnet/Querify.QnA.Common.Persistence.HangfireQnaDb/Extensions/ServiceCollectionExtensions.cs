using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Querify.Common.Infrastructure.Hangfire.Extensions;
using Querify.QnA.Common.Persistence.HangfireQnaDb.Configuration;
using Querify.QnA.Common.Persistence.HangfireQnaDb.DbContext;

namespace Querify.QnA.Common.Persistence.HangfireQnaDb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHangfireQnaDb(
        this IServiceCollection services,
        IConfiguration configuration,
        string[]? queues = null)
    {
        var connectionString = HangfireQnaDbConfiguration.GetConnectionString(configuration);
        var migrationsAssembly = typeof(HangfireQnaDbContext).Assembly.GetName().Name;

        services.AddDbContext<HangfireQnaDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                builder => builder
                    .EnableRetryOnFailure()
                    .MigrationsAssembly(migrationsAssembly)));

        services.AddHangFire(configuration, queues, connectionString);

        return services;
    }
}
