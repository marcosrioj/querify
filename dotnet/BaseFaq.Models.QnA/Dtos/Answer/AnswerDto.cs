using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.Models.QnA.Dtos.Answer;

public class AnswerDto
{
    public required Guid Id { get; set; }
    public required Guid TenantId { get; set; }
    public required Guid QuestionId { get; set; }
    public required string Headline { get; set; } = string.Empty;
    public string? Body { get; set; }
    public required AnswerKind Kind { get; set; }
    public required AnswerStatus Status { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public string? Language { get; set; }
    public string? ContextKey { get; set; }
    public string? ApplicabilityRulesJson { get; set; }
    public string? TrustNote { get; set; }
    public string? EvidenceSummary { get; set; }
    public string? AuthorLabel { get; set; }
    public required int ConfidenceScore { get; set; }
    public required int Rank { get; set; }
    public required int RevisionNumber { get; set; }
    public required bool IsAccepted { get; set; }
    public required bool IsOfficial { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? ValidatedAtUtc { get; set; }
    public DateTime? AcceptedAtUtc { get; set; }
    public DateTime? RetiredAtUtc { get; set; }
    public required int VoteScore { get; set; }
    public IReadOnlyList<AnswerSourceLinkDto> Sources { get; set; } = [];
}
