using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;

namespace BaseFaq.Tenant.Public.Api.Infrastructure;

public sealed class TenantPublicSessionService : ISessionService
{
    public Guid GetTenantId(ModuleEnum module)
    {
        throw new InvalidOperationException(
            "Tenant session context is not available in BaseFaq.Tenant.Public.Api. Public billing ingress must not depend on request-bound tenant headers.");
    }

    public Guid GetUserId() => Guid.Empty;
}
