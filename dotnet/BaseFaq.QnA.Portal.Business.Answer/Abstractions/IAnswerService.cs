using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Answer;

namespace BaseFaq.QnA.Portal.Business.Answer.Abstractions;

public interface IAnswerService
{
    Task<Guid> Create(AnswerCreateRequestDto dto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
    Task<PagedResultDto<AnswerDto>> GetAll(AnswerGetAllRequestDto requestDto, CancellationToken token);
    Task<AnswerDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, AnswerUpdateRequestDto dto, CancellationToken token);
    Task<Guid> Publish(Guid id, CancellationToken token);
    Task<Guid> Validate(Guid id, CancellationToken token);
    Task<Guid> Reject(Guid id, CancellationToken token);
    Task<Guid> Retire(Guid id, CancellationToken token);
    Task<Guid> AddSource(AnswerSourceLinkCreateRequestDto dto, CancellationToken token);
    Task RemoveSource(Guid answerId, Guid sourceLinkId, CancellationToken token);
}