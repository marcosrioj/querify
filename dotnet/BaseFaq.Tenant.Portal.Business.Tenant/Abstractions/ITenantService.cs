using BaseFaq.Models.Tenant.Dtos.Tenant;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;

public interface ITenantService
{
    Task<List<TenantSummaryDto>> GetAll(CancellationToken token);
    Task<bool> CreateOrUpdate(TenantCreateOrUpdateRequestDto requestDto, CancellationToken token);
    Task<bool> RefreshAllowedTenantCache(CancellationToken token);
    Task<string?> GetClientKey(Guid tenantId, CancellationToken token);
    Task<string> GenerateNewClientKey(Guid tenantId, CancellationToken token);
}
