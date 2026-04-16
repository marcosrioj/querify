using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;

namespace BaseFaq.QnA.Portal.Test.IntegrationTests.Helpers;

public sealed class TestSessionService(Guid tenantId, Guid userId) : ISessionService
{
    public Guid TenantId { get; } = tenantId;
    public Guid UserId { get; } = userId;

    public Guid GetTenantId(AppEnum app) => TenantId;

    public Guid GetUserId() => UserId;
}
