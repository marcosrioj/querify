using Querify.Models.Common.Enums;

namespace Querify.Common.Infrastructure.Signalr.Portal.Models;

public static class PortalNotificationGroups
{
    public static string TenantModule(Guid tenantId, ModuleEnum module)
    {
        return $"tenant:{tenantId:N}:module:{module}";
    }

    public static string User(Guid userId)
    {
        return $"user:{userId:N}";
    }
}
