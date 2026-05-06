using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Question;

namespace Querify.QnA.Public.Business.Question.Abstractions;

public interface IQuestionService
{
    Task<PagedResultDto<QuestionDto>> GetAll(QuestionGetAllRequestDto requestDto, CancellationToken token);
    Task<QuestionDetailDto> GetById(Guid id, QuestionGetRequestDto requestDto, CancellationToken token);
    Task<Guid> Create(QuestionCreateRequestDto dto, CancellationToken token);
}
