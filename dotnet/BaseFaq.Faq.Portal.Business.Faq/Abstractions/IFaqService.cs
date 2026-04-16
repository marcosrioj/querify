using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Faq;

namespace BaseFaq.Faq.Portal.Business.Faq.Abstractions;

public interface IFaqService
{
    Task<Guid> Create(FaqCreateRequestDto dto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
    Task<PagedResultDto<FaqDto>> GetAll(FaqGetAllRequestDto requestDto, CancellationToken token);
    Task<FaqDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, FaqUpdateRequestDto dto, CancellationToken token);
}
