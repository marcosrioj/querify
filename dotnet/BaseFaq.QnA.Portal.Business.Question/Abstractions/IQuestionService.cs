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
    Task<Guid> AddTag(QuestionTagCreateRequestDto dto, CancellationToken token);
    Task RemoveTag(Guid questionId, Guid tagId, CancellationToken token);
    Task<Guid> AddSource(QuestionSourceLinkCreateRequestDto dto, CancellationToken token);
    Task RemoveSource(Guid questionId, Guid sourceLinkId, CancellationToken token);
}
