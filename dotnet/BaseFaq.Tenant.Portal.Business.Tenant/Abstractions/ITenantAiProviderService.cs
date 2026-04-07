using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;

public interface ITenantAiProviderService
{
    Task<List<TenantAiProviderDto>> GetConfiguredAiProviders(Guid tenantId, CancellationToken token);
    Task<bool> IsAiProviderKeyConfigured(Guid tenantId, AiCommandType command, CancellationToken token);
}
