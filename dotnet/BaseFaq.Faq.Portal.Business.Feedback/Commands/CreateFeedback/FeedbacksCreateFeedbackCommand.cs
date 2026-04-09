using BaseFaq.Models.Faq.Enums;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Feedback.Commands.CreateFeedback;

public sealed class FeedbacksCreateFeedbackCommand : IRequest<Guid>
{
    public required bool Like { get; set; }
    public UnLikeReason? UnLikeReason { get; set; }
    public required Guid FaqItemId { get; set; }
}