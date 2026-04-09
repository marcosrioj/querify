using BaseFaq.Models.Faq.Dtos.Feedback;

namespace BaseFaq.Faq.Public.Business.Feedback.Abstractions;

public interface IFeedbackService
{
    Task<Guid> Feedback(FeedbackCreateRequestDto requestDto, CancellationToken token);
}