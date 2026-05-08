using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Querify.Common.Infrastructure.Signalr.Portal.Abstractions;
using Querify.Common.Infrastructure.Signalr.Portal.Options;
using Querify.Common.Infrastructure.Signalr.Portal.Publishers;
using Querify.Models.Common.Enums;

namespace Querify.Common.Infrastructure.Signalr.Portal.Extensions;

public static class PortalSignalRServiceCollectionExtensions
{
    public static IServiceCollection AddPortalSignalR(
        this IServiceCollection services,
        IConfiguration configuration,
        ModuleEnum module)
    {
        services.Configure<PortalSignalROptions>(configuration.GetSection(PortalSignalROptions.SectionName));
        services.PostConfigure<PortalSignalROptions>(options =>
        {
            options.Module = module;
            options.Normalize();
        });
        services.AddSignalR();
        services.AddScoped<IPortalNotificationPublisher, SignalRPortalNotificationPublisher>();

        return services;
    }
}
