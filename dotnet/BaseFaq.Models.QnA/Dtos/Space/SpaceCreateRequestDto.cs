using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Space;

public class SpaceCreateRequestDto
{
    public required string Name { get; set; } = string.Empty;
    public required string Key { get; set; } = string.Empty;
    public required string DefaultLanguage { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public required SpaceKind Kind { get; set; } = SpaceKind.CuratedKnowledge;
    public required VisibilityScope Visibility { get; set; } = VisibilityScope.Internal;
    public required ModerationPolicy ModerationPolicy { get; set; } = ModerationPolicy.PreModeration;
    public required SearchMarkupMode SearchMarkupMode { get; set; } = SearchMarkupMode.Off;
    public string? ProductScope { get; set; }
    public string? JourneyScope { get; set; }
    public required bool AcceptsQuestions { get; set; }
    public required bool AcceptsAnswers { get; set; }
    public required bool RequiresQuestionReview { get; set; } = true;
    public required bool RequiresAnswerReview { get; set; } = true;
    public required bool MarkValidated { get; set; }
}
