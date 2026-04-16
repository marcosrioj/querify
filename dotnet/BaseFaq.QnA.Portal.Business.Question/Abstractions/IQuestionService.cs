using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Question;

namespace BaseFaq.QnA.Portal.Business.Question.Abstractions;

public interface IQuestionService
{
    Task<Guid> Create(QuestionCreateRequestDto dto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
    Task<PagedResultDto<QuestionDto>> GetAll(QuestionGetAllRequestDto requestDto, CancellationToken token);
    Task<QuestionDetailDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, QuestionUpdateRequestDto dto, CancellationToken token);
    Task<Guid> Submit(Guid id, CancellationToken token);
    Task<Guid> Approve(Guid id, CancellationToken token);
    Task<Guid> Reject(Guid id, string? notes, CancellationToken token);
    Task<Guid> Escalate(Guid id, string? notes, CancellationToken token);
    Task<Guid> AddTopic(QuestionTopicCreateRequestDto dto, CancellationToken token);
    Task RemoveTopic(Guid questionId, Guid topicId, CancellationToken token);
    Task<Guid> AddSource(QuestionSourceLinkCreateRequestDto dto, CancellationToken token);
    Task RemoveSource(Guid questionId, Guid sourceLinkId, CancellationToken token);
}