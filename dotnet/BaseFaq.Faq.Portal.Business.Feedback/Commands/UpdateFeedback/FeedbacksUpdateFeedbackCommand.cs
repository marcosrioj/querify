using BaseFaq.Models.Faq.Enums;
using MediatR;

namespace BaseFaq.Faq.Portal.Business.Feedback.Commands.UpdateFeedback;

public sealed class FeedbacksUpdateFeedbackCommand : IRequest
{
    public required Guid Id { get; set; }
    public required bool Like { get; set; }
    public UnLikeReason? UnLikeReason { get; set; }
    public required Guid FaqItemId { get; set; }
}