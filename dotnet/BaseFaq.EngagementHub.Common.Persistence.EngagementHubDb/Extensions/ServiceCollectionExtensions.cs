using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.EngagementHub.Common.Persistence.EngagementHubDb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEngagementHubDb(this IServiceCollection services)
    {
        services.AddDbContext<EngagementHubDbContext>();

        return services;
    }
}
