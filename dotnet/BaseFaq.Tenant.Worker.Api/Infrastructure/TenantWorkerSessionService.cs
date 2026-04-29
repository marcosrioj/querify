using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using System.Net;

namespace BaseFaq.Tenant.Worker.Api.Infrastructure;

public sealed class TenantWorkerSessionService : ISessionService
{
    public Guid GetTenantId(ModuleEnum module)
    {
        throw new ApiErrorException(
            "Tenant context is not available in BaseFaq.Tenant.Worker.Api. Control-plane processing must not rely on request-bound tenant session state.",
            (int)HttpStatusCode.InternalServerError);
    }

    public Guid GetUserId() => Guid.Empty;
}
