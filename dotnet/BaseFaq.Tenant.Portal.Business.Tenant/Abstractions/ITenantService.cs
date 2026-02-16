using BaseFaq.Models.Tenant.Dtos.Tenant;
using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;

public interface ITenantService
{
    Task<List<TenantSummaryDto>> GetAll(CancellationToken token);
    Task<bool> CreateOrUpdate(TenantCreateOrUpdateRequestDto requestDto, CancellationToken token);
    Task<string?> GetClientKey(CancellationToken token);
    Task<string> GenerateNewClientKey(CancellationToken token);
    Task<List<TenantAiProviderDto>> GetConfiguredAiProviders(CancellationToken token);
    Task SetAiProviderCredentials(TenantSetAiProviderCredentialsRequestDto requestDto, CancellationToken token);
    Task<bool> IsAiProviderKeyConfigured(AiCommandType command, CancellationToken token);
}