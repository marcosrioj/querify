using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Source;

namespace BaseFaq.QnA.Portal.Business.Source.Abstractions;

public interface ISourceService
{
    Task<Guid> Create(SourceCreateRequestDto dto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);

    Task<PagedResultDto<SourceDto>>
        GetAll(SourceGetAllRequestDto requestDto, CancellationToken token);

    Task<SourceDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, SourceUpdateRequestDto dto, CancellationToken token);
}