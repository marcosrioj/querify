using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using System.Net;

namespace Querify.Tenant.Worker.Api.Infrastructure;

public sealed class TenantWorkerSessionService : ISessionService
{
    public Guid GetTenantId(ModuleEnum module)
    {
        throw new ApiErrorException(
            "Tenant context is not available in Querify.Tenant.Worker.Api. Control-plane processing must not rely on request-bound tenant session state.",
            (int)HttpStatusCode.InternalServerError);
    }

    public Guid GetUserId() => Guid.Empty;

    public string? GetUserName() => "tenant-worker";
}
