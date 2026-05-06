using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using System.Net;

namespace Querify.Tenant.Public.Api.Infrastructure;

public sealed class TenantPublicSessionService : ISessionService
{
    public Guid GetTenantId(ModuleEnum module)
    {
        throw new ApiErrorException(
            "Tenant session context is not available in Querify.Tenant.Public.Api. Public billing ingress must not depend on request-bound tenant headers.",
            (int)HttpStatusCode.InternalServerError);
    }

    public Guid GetUserId() => Guid.Empty;

    public string? GetUserName() => "public";
}
