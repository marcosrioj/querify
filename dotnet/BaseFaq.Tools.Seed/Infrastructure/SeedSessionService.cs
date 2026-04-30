using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;

namespace BaseFaq.Tools.Seed.Infrastructure;

public sealed class SeedSessionService(Guid userId, Guid tenantId, string? userName = "seed") : ISessionService
{
    public Guid GetTenantId(ModuleEnum module)
    {
        return tenantId;
    }

    public Guid GetUserId()
    {
        return userId;
    }

    public string? GetUserName()
    {
        return userName;
    }
}
