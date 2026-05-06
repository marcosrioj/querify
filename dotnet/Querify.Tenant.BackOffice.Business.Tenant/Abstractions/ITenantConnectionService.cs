using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.TenantConnection;

namespace Querify.Tenant.BackOffice.Business.Tenant.Abstractions;

public interface ITenantConnectionService
{
    Task<Guid> Create(TenantConnectionCreateRequestDto requestDto, CancellationToken token);

    Task Delete(Guid id, CancellationToken token);

    Task<PagedResultDto<TenantConnectionDto>> GetAll(TenantConnectionGetAllRequestDto requestDto,
        CancellationToken token);

    Task<TenantConnectionDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, TenantConnectionUpdateRequestDto requestDto, CancellationToken token);
}