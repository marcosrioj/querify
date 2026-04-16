using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Question;

public class QuestionSourceLinkUpdateRequestDto
{
    public Guid QuestionId { get; set; }
    public Guid SourceId { get; set; }
    public SourceRole Role { get; set; } = SourceRole.QuestionOrigin;
    public int Order { get; set; }
}
