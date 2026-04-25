using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;

namespace BaseFaq.Tenant.Worker.Api.Infrastructure;

public sealed class TenantWorkerSessionService : ISessionService
{
    public Guid GetTenantId(ModuleEnum module)
    {
        throw new InvalidOperationException(
            "Tenant context is not available in BaseFaq.Tenant.Worker.Api. Control-plane processing must not rely on request-bound tenant session state.");
    }

    public Guid GetUserId() => Guid.Empty;
}
