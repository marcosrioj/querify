using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Question;

namespace BaseFaq.QnA.Public.Business.Question.Abstractions;

public interface IQuestionService
{
    Task<PagedResultDto<QuestionDto>> GetAll(QuestionGetAllRequestDto requestDto, CancellationToken token);
    Task<QuestionDetailDto> GetById(Guid id, QuestionGetRequestDto requestDto, CancellationToken token);
    Task<QuestionDetailDto> GetByKey(string key, QuestionGetRequestDto requestDto, CancellationToken token);
    Task<Guid> Create(QuestionCreateRequestDto dto, CancellationToken token);
}