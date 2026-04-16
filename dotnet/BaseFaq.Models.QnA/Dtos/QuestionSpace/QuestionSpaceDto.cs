using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.QuestionSpace;

public class QuestionSpaceDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string DefaultLanguage { get; set; } = string.Empty;
    public SpaceKind Kind { get; set; }
    public VisibilityScope Visibility { get; set; }
    public ModerationPolicy ModerationPolicy { get; set; }
    public SearchMarkupMode SearchMarkupMode { get; set; }
    public string? ProductScope { get; set; }
    public string? JourneyScope { get; set; }
    public bool AcceptsQuestions { get; set; }
    public bool AcceptsAnswers { get; set; }
    public bool RequiresQuestionReview { get; set; }
    public bool RequiresAnswerReview { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? LastValidatedAtUtc { get; set; }
    public int QuestionCount { get; set; }
}
