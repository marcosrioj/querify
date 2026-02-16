using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Abstractions;

public interface ITenantAiProviderService
{
    Task<Guid> Create(TenantAiProviderCreateRequestDto requestDto, CancellationToken token);
    Task<Guid> Update(Guid id, TenantAiProviderUpdateRequestDto requestDto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
    Task<TenantAiProviderDto> GetById(Guid id, CancellationToken token);

    Task<PagedResultDto<TenantAiProviderDto>> GetAll(TenantAiProviderGetAllRequestDto requestDto,
        CancellationToken token);
}