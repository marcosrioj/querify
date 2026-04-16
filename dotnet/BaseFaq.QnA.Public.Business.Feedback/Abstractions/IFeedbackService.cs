using BaseFaq.Models.QnA.Dtos.Question;

namespace BaseFaq.QnA.Public.Business.Feedback.Abstractions;

public interface IFeedbackService
{
    Task<Guid> Create(QuestionFeedbackCreateRequestDto dto, CancellationToken token);
}
