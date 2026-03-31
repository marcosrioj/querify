using BaseFaq.Common.Infrastructure.Core.Abstractions;
using BaseFaq.Models.Common.Enums;

namespace BaseFaq.AI.Api.Infrastructure;

public sealed class AiWorkerSessionService : ISessionService
{
    public Guid GetTenantId(AppEnum app)
    {
        throw new InvalidOperationException(
            "AI worker session tenant is not available. Use tenant-aware message payloads with IFaqDbContextFactory.");
    }

    public Guid GetUserId() => Guid.Empty;
}
