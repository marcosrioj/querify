using Querify.Broadcast.Common.Persistence.BroadcastDb.DbContext;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Broadcast.Common.Persistence.BroadcastDb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBroadcastDb(this IServiceCollection services)
    {
        services.AddDbContext<BroadcastDbContext>();

        return services;
    }
}
