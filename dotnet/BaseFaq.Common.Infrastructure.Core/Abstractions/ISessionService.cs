using BaseFaq.Models.Common.Enums;

namespace BaseFaq.Common.Infrastructure.Core.Abstractions;

public interface ISessionService
{
    Guid GetTenantId(ModuleEnum module);
    Guid GetUserId();
    string? GetUserName();
}
