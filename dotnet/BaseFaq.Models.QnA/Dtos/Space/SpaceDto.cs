using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Space;

public class SpaceDto
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required string Name { get; set; } = string.Empty;
    public required string Key { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public required string DefaultLanguage { get; set; } = string.Empty;
    public required SpaceKind Kind { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public required ModerationPolicy ModerationPolicy { get; set; }
    public required SearchMarkupMode SearchMarkupMode { get; set; }
    public string? ProductScope { get; set; }
    public string? JourneyScope { get; set; }
    public required bool AcceptsQuestions { get; set; }
    public required bool AcceptsAnswers { get; set; }
    public required bool RequiresQuestionReview { get; set; }
    public required bool RequiresAnswerReview { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? LastValidatedAtUtc { get; set; }
    public required int QuestionCount { get; set; }
}
