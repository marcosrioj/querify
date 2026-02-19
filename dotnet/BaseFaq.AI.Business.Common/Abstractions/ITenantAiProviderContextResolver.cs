using BaseFaq.AI.Business.Common.Models;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.AI.Business.Common.Abstractions;

public interface ITenantAiProviderContextResolver
{
    Task<AiProviderContext> ResolveAsync(Guid tenantId, AiCommandType commandType, CancellationToken cancellationToken);
}
