using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.Tenant;

namespace Querify.Tenant.BackOffice.Business.Tenant.Abstractions;

public interface ITenantService
{
    Task<Guid> Create(TenantCreateRequestDto requestDto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
    Task<PagedResultDto<TenantDto>> GetAll(TenantGetAllRequestDto requestDto, CancellationToken token);
    Task<TenantDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, TenantUpdateRequestDto requestDto, CancellationToken token);
}