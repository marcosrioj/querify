using BaseFaq.AI.Business.Common.Models;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.AI.Business.Common.Abstractions;

public interface IAiProviderContextResolver
{
    Task<AiProviderContext> ResolveAsync(
        Guid tenantId,
        AiCommandType commandType,
        CancellationToken cancellationToken = default);
}