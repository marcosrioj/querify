using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Answer;

public class AnswerSourceLinkUpdateRequestDto
{
    public Guid AnswerId { get; set; }
    public Guid SourceId { get; set; }
    public SourceRole Role { get; set; } = SourceRole.Evidence;
    public string? Label { get; set; }
    public string? Scope { get; set; }
    public string? Excerpt { get; set; }
    public int Order { get; set; }
    public int ConfidenceScore { get; set; }
    public bool IsPrimary { get; set; }
}
