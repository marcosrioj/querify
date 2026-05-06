using Querify.Trust.Common.Persistence.TrustDb.DbContext;
using Microsoft.Extensions.DependencyInjection;

namespace Querify.Trust.Common.Persistence.TrustDb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTrustDb(this IServiceCollection services)
    {
        services.AddDbContext<TrustDbContext>();

        return services;
    }
}
