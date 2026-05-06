using Querify.Models.Tenant.Dtos.TenantUser;

namespace Querify.Tenant.BackOffice.Business.Tenant.Abstractions;

public interface ITenantUserService
{
    Task<List<TenantUserDto>> GetAll(Guid tenantId, CancellationToken token);
    Task<Guid> Create(Guid tenantId, TenantUserCreateRequestDto requestDto, CancellationToken token);
    Task<Guid> Update(Guid tenantId, Guid id, TenantUserUpdateRequestDto requestDto, CancellationToken token);
    Task Delete(Guid tenantId, Guid id, CancellationToken token);
}
