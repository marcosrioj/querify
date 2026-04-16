using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Abstractions;

public interface IQuestionSpaceService
{
    Task<Guid> Create(QuestionSpaceCreateRequestDto dto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
    Task<PagedResultDto<QuestionSpaceDto>> GetAll(QuestionSpaceGetAllRequestDto requestDto, CancellationToken token);
    Task<QuestionSpaceDetailDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, QuestionSpaceUpdateRequestDto dto, CancellationToken token);
    Task<Guid> AddTopic(QuestionSpaceTopicCreateRequestDto dto, CancellationToken token);
    Task RemoveTopic(Guid questionSpaceId, Guid topicId, CancellationToken token);
    Task<Guid> AddCuratedSource(QuestionSpaceSourceCreateRequestDto dto, CancellationToken token);
    Task RemoveCuratedSource(Guid questionSpaceId, Guid knowledgeSourceId, CancellationToken token);
}