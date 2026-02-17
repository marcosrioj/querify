using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;

public interface ITenantAiProviderService
{
    Task<List<TenantAiProviderDto>> GetConfiguredAiProviders(CancellationToken token);
    Task<bool> IsAiProviderKeyConfigured(AiCommandType command, CancellationToken token);
}