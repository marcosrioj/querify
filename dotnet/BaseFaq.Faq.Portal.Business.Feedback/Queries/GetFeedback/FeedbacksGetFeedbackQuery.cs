using BaseFaq.Models.Faq.Dtos.Feedback;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Feedback.Queries.GetFeedback;

public sealed class FeedbacksGetFeedbackQuery : IRequest<FeedbackDto?>
{
    public required Guid Id { get; set; }
}