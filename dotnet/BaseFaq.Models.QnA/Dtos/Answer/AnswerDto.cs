using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Answer;

public class AnswerDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid QuestionId { get; set; }
    public string Headline { get; set; } = string.Empty;
    public string? Body { get; set; }
    public AnswerKind Kind { get; set; }
    public AnswerStatus Status { get; set; }
    public VisibilityScope Visibility { get; set; }
    public string? Language { get; set; }
    public string? ContextKey { get; set; }
    public string? ApplicabilityRulesJson { get; set; }
    public string? TrustNote { get; set; }
    public string? EvidenceSummary { get; set; }
    public string? AuthorLabel { get; set; }
    public int ConfidenceScore { get; set; }
    public int Rank { get; set; }
    public int RevisionNumber { get; set; }
    public bool IsAccepted { get; set; }
    public bool IsCanonical { get; set; }
    public bool IsOfficial { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? ValidatedAtUtc { get; set; }
    public DateTime? AcceptedAtUtc { get; set; }
    public DateTime? RetiredAtUtc { get; set; }
    public int VoteScore { get; set; }
    public IReadOnlyList<AnswerSourceLinkDto> Sources { get; set; } = [];
}
