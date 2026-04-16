using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;

namespace BaseFaq.QnA.Public.Business.QuestionSpace.Abstractions;

public interface IQuestionSpaceService
{
    Task<PagedResultDto<QuestionSpaceDto>> GetAll(QuestionSpaceGetAllRequestDto requestDto, CancellationToken token);
    Task<QuestionSpaceDto> GetById(Guid id, CancellationToken token);
    Task<QuestionSpaceDto> GetByKey(string key, CancellationToken token);
}