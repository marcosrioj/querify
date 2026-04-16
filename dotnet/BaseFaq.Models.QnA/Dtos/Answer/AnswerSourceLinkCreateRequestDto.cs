using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Answer;

public class AnswerSourceLinkCreateRequestDto
{
    public required Guid AnswerId { get; set; }
    public required Guid SourceId { get; set; }
    public required SourceRole Role { get; set; } = SourceRole.Evidence;
    public required int Order { get; set; }
}
