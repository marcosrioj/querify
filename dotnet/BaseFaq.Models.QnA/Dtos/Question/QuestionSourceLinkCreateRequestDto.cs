using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionSourceLinkCreateRequestDto
{
    public Guid QuestionId { get; set; }
    public Guid SourceId { get; set; }
    public SourceRole Role { get; set; } = SourceRole.QuestionOrigin;
    public int Order { get; set; }
    public int ConfidenceScore { get; set; }
    public bool IsPrimary { get; set; }
}
