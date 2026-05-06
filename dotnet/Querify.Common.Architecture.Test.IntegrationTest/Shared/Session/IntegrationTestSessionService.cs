using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;

namespace Querify.Common.Architecture.Test.IntegrationTest.Shared.Session;

public sealed class IntegrationTestSessionService(
    Guid tenantId,
    Guid userId,
    string? userName = "Integration Test User") : ISessionService
{
    public Guid TenantId { get; } = tenantId;

    public Guid UserId { get; } = userId;

    public string? UserName { get; } = userName;

    public Guid GetTenantId(ModuleEnum module)
    {
        return TenantId;
    }

    public Guid GetUserId()
    {
        return UserId;
    }

    public string? GetUserName()
    {
        return UserName;
    }
}
