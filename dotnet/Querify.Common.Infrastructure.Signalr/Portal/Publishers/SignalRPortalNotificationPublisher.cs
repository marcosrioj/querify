using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Querify.Common.Infrastructure.Signalr.Portal.Abstractions;
using Querify.Common.Infrastructure.Signalr.Portal.Hubs;
using Querify.Common.Infrastructure.Signalr.Portal.Models;
using Querify.Common.Infrastructure.Signalr.Portal.Options;
using Querify.Models.Common.Enums;

namespace Querify.Common.Infrastructure.Signalr.Portal.Publishers;

public sealed class SignalRPortalNotificationPublisher(
    IHubContext<PortalNotificationsHub> hubContext,
    IOptions<PortalSignalROptions> options)
    : IPortalNotificationPublisher
{
    public Task PublishToTenantModuleAsync(
        ModuleEnum module,
        Guid tenantId,
        PortalNotificationEnvelope envelope,
        CancellationToken cancellationToken)
    {
        return hubContext.Clients
            .Group(PortalNotificationGroups.TenantModule(tenantId, module))
            .SendAsync(options.Value.NotificationClientMethod, envelope, cancellationToken);
    }
}
