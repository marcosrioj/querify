using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;

namespace BaseFaq.Tenant.Worker.Test.IntegrationTests.Helpers;

public sealed class TestSessionService : ISessionService
{
    public TestSessionService(Guid tenantId, Guid userId)
    {
        TenantId = tenantId;
        UserId = userId;
    }

    public Guid TenantId { get; }
    public Guid UserId { get; }

    public Guid GetTenantId(AppEnum app) => TenantId;

    public Guid GetUserId() => UserId;
}
