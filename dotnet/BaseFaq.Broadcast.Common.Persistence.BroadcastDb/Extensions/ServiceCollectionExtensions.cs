using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBroadcastDb(this IServiceCollection services)
    {
        services.AddDbContext<BroadcastDbContext>();

        return services;
    }
}
