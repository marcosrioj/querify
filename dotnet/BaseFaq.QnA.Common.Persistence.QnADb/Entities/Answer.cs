using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

public class Answer : BaseEntity, IMustHaveTenant
{
    public const int MaxHeadlineLength = 250;
    public const int MaxBodyLength = 6000;
    public const int MaxLanguageLength = 50;
    public const int MaxContextKeyLength = 200;
    public const int MaxApplicabilityRulesLength = 4000;
    public const int MaxTrustNoteLength = 2000;
    public const int MaxEvidenceSummaryLength = 4000;
    public const int MaxAuthorLabelLength = 200;

    public required Guid TenantId { get; set; }
    public required string Headline { get; set; } = null!;
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
    public int RevisionNumber { get; set; }
    public bool IsAccepted { get; set; }
    public bool IsCanonical { get; set; }
    public bool IsOfficial { get; set; }
    public required Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? ValidatedAtUtc { get; set; }
    public DateTime? AcceptedAtUtc { get; set; }
    public DateTime? RetiredAtUtc { get; set; }
    public ICollection<AnswerSourceLink> Sources { get; set; } = [];
    public ICollection<ThreadActivity> Activity { get; set; } = [];
}
