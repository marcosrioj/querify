using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using System.Net;

namespace BaseFaq.Tenant.Public.Api.Infrastructure;

public sealed class TenantPublicSessionService : ISessionService
{
    public Guid GetTenantId(ModuleEnum module)
    {
        throw new ApiErrorException(
            "Tenant session context is not available in BaseFaq.Tenant.Public.Api. Public billing ingress must not depend on request-bound tenant headers.",
            (int)HttpStatusCode.InternalServerError);
    }

    public Guid GetUserId() => Guid.Empty;
}
