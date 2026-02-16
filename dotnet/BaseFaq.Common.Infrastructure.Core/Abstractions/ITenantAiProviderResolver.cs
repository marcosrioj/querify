using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Common.Infrastructure.Core.Abstractions;

public interface ITenantAiProviderResolver
{
    Task<bool> HasProviderForCommandAsync(
        Guid tenantId,
        AiCommandType commandType,
        CancellationToken cancellationToken = default);
}