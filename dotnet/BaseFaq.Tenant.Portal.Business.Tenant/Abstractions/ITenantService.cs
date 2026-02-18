using BaseFaq.Models.Tenant.Dtos.Tenant;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;

public interface ITenantService
{
    Task<List<TenantSummaryDto>> GetAll(CancellationToken token);
    Task<bool> CreateOrUpdate(TenantCreateOrUpdateRequestDto requestDto, CancellationToken token);
    Task<string?> GetClientKey(CancellationToken token);
    Task<string> GenerateNewClientKey(CancellationToken token);
    Task<bool> SetAiProviderCredentials(TenantSetAiProviderCredentialsRequestDto requestDto, CancellationToken token);
}