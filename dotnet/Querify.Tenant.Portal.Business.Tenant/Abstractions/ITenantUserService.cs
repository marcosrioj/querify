using Querify.Models.Tenant.Dtos.TenantUser;

namespace Querify.Tenant.Portal.Business.Tenant.Abstractions;

public interface ITenantUserService
{
    Task<List<TenantUserDto>> GetAll(Guid tenantId, CancellationToken token);
    Task<Guid> AddTenantMember(TenantUserCreateRequestDto requestDto, CancellationToken token);
    Task Delete(Guid tenantId, Guid id, CancellationToken token);
}
