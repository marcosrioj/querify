using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.QuestionSpace;

public class QuestionSpaceUpdateRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string DefaultLanguage { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public SpaceKind Kind { get; set; } = SpaceKind.CuratedKnowledge;
    public VisibilityScope Visibility { get; set; } = VisibilityScope.Internal;
    public ModerationPolicy ModerationPolicy { get; set; } = ModerationPolicy.PreModeration;
    public SearchMarkupMode SearchMarkupMode { get; set; } = SearchMarkupMode.Off;
    public string? ProductScope { get; set; }
    public string? JourneyScope { get; set; }
    public bool AcceptsQuestions { get; set; }
    public bool AcceptsAnswers { get; set; }
    public bool RequiresQuestionReview { get; set; } = true;
    public bool RequiresAnswerReview { get; set; } = true;
    public bool MarkValidated { get; set; }
}
