using Querify.Models.QnA.Dtos.Question;

namespace Querify.QnA.Public.Business.Feedback.Abstractions;

public interface IFeedbackService
{
    Task<Guid> Create(QuestionFeedbackCreateRequestDto dto, CancellationToken token);
}