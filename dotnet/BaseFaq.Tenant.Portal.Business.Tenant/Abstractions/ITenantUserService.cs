using BaseFaq.Models.Tenant.Dtos.TenantUser;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;

public interface ITenantUserService
{
    Task<List<TenantUserDto>> GetAll(CancellationToken token);
    Task<Guid> Create(TenantUserCreateRequestDto requestDto, CancellationToken token);
    Task<Guid> Update(Guid id, TenantUserUpdateRequestDto requestDto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
}
