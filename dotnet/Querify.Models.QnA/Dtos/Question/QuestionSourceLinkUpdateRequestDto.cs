using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.Question;

public class QuestionSourceLinkUpdateRequestDto
{
    public required Guid QuestionId { get; set; }
    public required Guid SourceId { get; set; }
    public required SourceRole Role { get; set; }
    public required int Order { get; set; }
}
