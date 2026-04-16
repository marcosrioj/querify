using BaseFaq.Models.Faq.Dtos.FaqItemAnswer;
using BaseFaq.Models.Faq.Enums;

namespace BaseFaq.Models.Faq.Dtos.FaqItem;

public class FaqItemDto
{
    public required Guid Id { get; set; }
    public required string Question { get; set; }
    public required string ShortAnswer { get; set; }
    public string? Answer { get; set; }
    public List<FaqItemAnswerDto> Answers { get; set; } = [];
    public string? AdditionalInfo { get; set; }
    public string? CtaTitle { get; set; }
    public string? CtaUrl { get; set; }
    public required int Sort { get; set; }
    public required int FeedbackScore { get; set; }
    public required int ConfidenceScore { get; set; }
    public required bool IsActive { get; set; }
    public required Guid FaqId { get; set; }
    public Guid? ContentRefId { get; set; }
}
