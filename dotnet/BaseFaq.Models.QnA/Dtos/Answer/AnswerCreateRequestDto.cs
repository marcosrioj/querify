using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Answer;

public class AnswerCreateRequestDto
{
    public required Guid QuestionId { get; set; }
    public required string Headline { get; set; } = string.Empty;
    public string? Body { get; set; }
    public required AnswerKind Kind { get; set; } = AnswerKind.Official;
    public required AnswerStatus Status { get; set; } = AnswerStatus.Draft;
    public required VisibilityScope Visibility { get; set; } = VisibilityScope.Internal;
    public string? Language { get; set; }
    public string? ContextKey { get; set; }
    public string? ApplicabilityRulesJson { get; set; }
    public string? TrustNote { get; set; }
    public string? EvidenceSummary { get; set; }
    public string? AuthorLabel { get; set; }
    public required int ConfidenceScore { get; set; }
    public required int Rank { get; set; }
}
