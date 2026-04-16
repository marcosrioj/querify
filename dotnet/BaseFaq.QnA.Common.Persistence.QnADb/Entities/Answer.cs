using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Represents a candidate, official, or curated answer for a question.
/// </summary>
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

    /// <summary>
    /// Short answer title for previews and quick reading.
    /// </summary>
    public required string Headline { get; set; } = null!;

    /// <summary>
    /// Detailed answer body.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Functional classification of the answer.
    /// </summary>
    public required AnswerKind Kind { get; set; } = AnswerKind.Official;

    /// <summary>
    /// Current workflow state of the answer.
    /// </summary>
    public required AnswerStatus Status { get; set; } = AnswerStatus.Draft;

    /// <summary>
    /// Visibility scope of the answer.
    /// </summary>
    public required VisibilityScope Visibility { get; set; } = VisibilityScope.Internal;

    /// <summary>
    /// Language of the answer variant.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Context key for plan, country, version, or integration variants.
    /// </summary>
    public string? ContextKey { get; set; }

    /// <summary>
    /// Serialized rules that determine in which context the answer applies.
    /// </summary>
    public string? ApplicabilityRulesJson { get; set; }

    /// <summary>
    /// Human-readable explanation of why the answer can be trusted.
    /// </summary>
    public string? TrustNote { get; set; }

    /// <summary>
    /// Consolidated summary of the evidence supporting the answer.
    /// </summary>
    public string? EvidenceSummary { get; set; }

    /// <summary>
    /// Public label for the author or origin of the answer.
    /// </summary>
    public string? AuthorLabel { get; set; }

    /// <summary>
    /// Calculated confidence level for the answer.
    /// </summary>
    public required int ConfidenceScore { get; set; }

    /// <summary>
    /// Relative ranking of the answer among other answers to the same question.
    /// </summary>
    public required int Rank { get; set; }

    /// <summary>
    /// Current revision of the answer.
    /// </summary>
    public required int RevisionNumber { get; set; }

    /// <summary>
    /// Id of the question that owns the answer.
    /// </summary>
    public required Guid QuestionId { get; set; }

    /// <summary>
    /// Question that owns the answer.
    /// </summary>
    public Question Question { get; set; } = null!;

    /// <summary>
    /// Timestamp when the answer was published.
    /// </summary>
    public DateTime? PublishedAtUtc { get; set; }

    /// <summary>
    /// Timestamp when the answer was validated.
    /// </summary>
    public DateTime? ValidatedAtUtc { get; set; }

    /// <summary>
    /// Timestamp when the answer was accepted as primary.
    /// </summary>
    public DateTime? AcceptedAtUtc { get; set; }

    /// <summary>
    /// Timestamp when the answer was retired.
    /// </summary>
    public DateTime? RetiredAtUtc { get; set; }

    /// <summary>
    /// Sources that support the answer.
    /// </summary>
    public ICollection<AnswerSourceLink> Sources { get; set; } = [];

    /// <summary>
    /// Tenant that owns the answer.
    /// </summary>
    public required Guid TenantId { get; set; }
}
