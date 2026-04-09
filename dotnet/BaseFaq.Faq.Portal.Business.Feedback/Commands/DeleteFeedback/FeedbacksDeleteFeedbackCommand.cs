using MediatR;

namespace BaseFaq.Faq.Portal.Business.Feedback.Commands.DeleteFeedback;

public sealed class FeedbacksDeleteFeedbackCommand : IRequest
{
    public required Guid Id { get; set; }
}