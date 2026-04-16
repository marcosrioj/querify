using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Abstractions;

public interface IKnowledgeSourceService
{
    Task<Guid> Create(KnowledgeSourceCreateRequestDto dto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);

    Task<PagedResultDto<KnowledgeSourceDto>>
        GetAll(KnowledgeSourceGetAllRequestDto requestDto, CancellationToken token);

    Task<KnowledgeSourceDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, KnowledgeSourceUpdateRequestDto dto, CancellationToken token);
}