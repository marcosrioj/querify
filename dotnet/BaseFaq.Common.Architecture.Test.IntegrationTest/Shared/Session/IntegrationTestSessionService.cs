using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;

namespace BaseFaq.Common.Architecture.Test.IntegrationTest.Shared.Session;

public sealed class IntegrationTestSessionService(Guid tenantId, Guid userId) : ISessionService
{
    public Guid TenantId { get; } = tenantId;

    public Guid UserId { get; } = userId;

    public Guid GetTenantId(ModuleEnum module)
    {
        return TenantId;
    }

    public Guid GetUserId()
    {
        return UserId;
    }
}
