using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Source;

public class SourceAnswerRelationshipDto
{
    public required Guid Id { get; set; }
    public required Guid AnswerId { get; set; }
    public required Guid QuestionId { get; set; }
    public required string QuestionTitle { get; set; }
    public required string Headline { get; set; }
    public required AnswerKind Kind { get; set; }
    public required AnswerStatus Status { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public required SourceRole Role { get; set; }
    public required int Order { get; set; }
    public required bool IsAccepted { get; set; }
}
