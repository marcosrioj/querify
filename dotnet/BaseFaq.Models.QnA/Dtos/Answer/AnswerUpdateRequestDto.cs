using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Answer;

public class AnswerUpdateRequestDto
{
    public Guid QuestionId { get; set; }
    public string Headline { get; set; } = string.Empty;
    public string? Body { get; set; }
    public AnswerKind Kind { get; set; } = AnswerKind.Official;
    public AnswerStatus Status { get; set; } = AnswerStatus.Draft;
    public VisibilityScope Visibility { get; set; } = VisibilityScope.Internal;
    public string? Language { get; set; }
    public string? ContextKey { get; set; }
    public string? ApplicabilityRulesJson { get; set; }
    public string? TrustNote { get; set; }
    public string? EvidenceSummary { get; set; }
    public string? AuthorLabel { get; set; }
    public int ConfidenceScore { get; set; }
    public int Rank { get; set; }
    public bool IsCanonical { get; set; }
    public bool IsOfficial { get; set; } = true;
}
