using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionSourceLinkUpdateRequestDto
{
    public Guid QuestionId { get; set; }
    public Guid SourceId { get; set; }
    public SourceRole Role { get; set; } = SourceRole.QuestionOrigin;
    public string? Label { get; set; }
    public string? Scope { get; set; }
    public string? Excerpt { get; set; }
    public int Order { get; set; }
    public int ConfidenceScore { get; set; }
    public bool IsPrimary { get; set; }
}
