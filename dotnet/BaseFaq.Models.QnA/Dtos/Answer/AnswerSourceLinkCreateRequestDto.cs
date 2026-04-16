using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Answer;

public class AnswerSourceLinkCreateRequestDto
{
    public Guid AnswerId { get; set; }
    public Guid SourceId { get; set; }
    public SourceRole Role { get; set; } = SourceRole.Evidence;
    public int Order { get; set; }
    public int ConfidenceScore { get; set; }
    public bool IsPrimary { get; set; }
}
