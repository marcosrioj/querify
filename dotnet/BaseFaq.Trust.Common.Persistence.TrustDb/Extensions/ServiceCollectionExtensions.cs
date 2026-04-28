using BaseFaq.Trust.Common.Persistence.TrustDb.DbContext;
using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.Trust.Common.Persistence.TrustDb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTrustDb(this IServiceCollection services)
    {
        services.AddDbContext<TrustDbContext>();

        return services;
    }
}
