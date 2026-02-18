using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;
using Microsoft.Extensions.Configuration;

namespace BaseFaq.AI.Persistence.AiDb.Infrastructure;

internal sealed class AiDbSessionService(IConfiguration configuration) : ISessionService
{
    public Guid GetTenantId(AppEnum app) => Guid.Empty;

    public Guid GetUserId()
    {
        return Guid.TryParse(configuration["Ai:UserId"], out var configuredUserId)
            ? configuredUserId
            : Guid.Empty;
    }
}