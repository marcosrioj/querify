using BaseFaq.Models.Faq.Enums;

namespace BaseFaq.Models.Faq.Dtos.Feedback;

public class FeedbackUpdateRequestDto
{
    public required bool Like { get; set; }
    public UnLikeReason? UnLikeReason { get; set; }
    public required Guid FaqItemId { get; set; }
}