using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.AiProvider;

namespace BaseFaq.Tenant.BackOffice.Business.AiProvider.Abstractions;

public interface IAiProviderService
{
    Task<Guid> Create(AiProviderCreateRequestDto requestDto, CancellationToken token);
    Task<Guid> Update(Guid id, AiProviderUpdateRequestDto requestDto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
    Task<AiProviderDto> GetById(Guid id, CancellationToken token);
    Task<PagedResultDto<AiProviderDto>> GetAll(AiProviderGetAllRequestDto requestDto, CancellationToken token);
}