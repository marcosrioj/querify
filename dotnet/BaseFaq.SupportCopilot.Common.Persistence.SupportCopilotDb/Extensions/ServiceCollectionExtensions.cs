using Microsoft.Extensions.DependencyInjection;

namespace BaseFaq.SupportCopilot.Common.Persistence.SupportCopilotDb.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSupportCopilotDb(this IServiceCollection services)
    {
        services.AddDbContext<SupportCopilotDbContext>();

        return services;
    }
}
