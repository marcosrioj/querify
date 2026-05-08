using Microsoft.Extensions.Configuration;
using Querify.Models.Common.Enums;

namespace Querify.Common.Infrastructure.Signalr.Portal.Options;

public sealed class PortalSignalROptions
{
    public const string SectionName = "PortalSignalR";
    public const string DefaultNotificationsHubPath = "/api/qna/hubs/portal-notifications";
    public const string DefaultNotificationClientMethod = "portalNotification";

    public string NotificationsHubPath { get; set; } = DefaultNotificationsHubPath;
    public string NotificationClientMethod { get; set; } = DefaultNotificationClientMethod;
    public ModuleEnum Module { get; set; } = ModuleEnum.QnA;

    public static PortalSignalROptions FromConfiguration(IConfiguration configuration, ModuleEnum module)
    {
        var options = configuration.GetSection(SectionName).Get<PortalSignalROptions>() ?? new PortalSignalROptions();
        options.Module = module;
        options.Normalize();

        return options;
    }

    internal void Normalize()
    {
        if (string.IsNullOrWhiteSpace(NotificationsHubPath))
        {
            NotificationsHubPath = DefaultNotificationsHubPath;
        }

        if (string.IsNullOrWhiteSpace(NotificationClientMethod))
        {
            NotificationClientMethod = DefaultNotificationClientMethod;
        }
    }
}
