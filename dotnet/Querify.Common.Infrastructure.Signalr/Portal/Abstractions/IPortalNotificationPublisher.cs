using Querify.Common.Infrastructure.Signalr.Portal.Models;
using Querify.Models.Common.Enums;

namespace Querify.Common.Infrastructure.Signalr.Portal.Abstractions;

public interface IPortalNotificationPublisher
{
    Task PublishToTenantModuleAsync(
        ModuleEnum module,
        Guid tenantId,
        PortalNotificationEnvelope envelope,
        CancellationToken cancellationToken);
}
