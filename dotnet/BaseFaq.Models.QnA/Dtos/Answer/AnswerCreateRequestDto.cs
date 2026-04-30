using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Answer;

public class AnswerCreateRequestDto
{
    public required Guid QuestionId { get; set; }
    public required string Headline { get; set; }
    public string? Body { get; set; }
    public required AnswerKind Kind { get; set; }
    public required AnswerStatus Status { get; set; }
    public VisibilityScope Visibility { get; set; } = VisibilityScope.Internal;
    public string? ContextNote { get; set; }
    public string? AuthorLabel { get; set; }
    public required int Sort { get; set; }
}
