using Querify.Models.Common.Enums;

namespace Querify.Common.Infrastructure.Core.Abstractions;

public interface ISessionService
{
    Guid GetTenantId(ModuleEnum module);
    Guid GetUserId();
    string? GetUserName();
}
