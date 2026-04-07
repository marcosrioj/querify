using BaseFaq.Models.Tenant.Dtos.TenantUser;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;

public interface ITenantUserService
{
    Task<List<TenantUserDto>> GetAll(Guid tenantId, CancellationToken token);
    Task<Guid> AddTenantMember(TenantUserCreateRequestDto requestDto, CancellationToken token);
    Task Delete(Guid tenantId, Guid id, CancellationToken token);
}
