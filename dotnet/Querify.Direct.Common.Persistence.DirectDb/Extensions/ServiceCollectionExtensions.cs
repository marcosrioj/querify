using Querify.Direct.Common.Persistence.DirectDb.DbContext;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Direct.Common.Persistence.DirectDb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDirectDb(this IServiceCollection services)
    {
        services.AddDbContext<DirectDbContext>();

        return services;
    }
}
