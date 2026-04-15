using BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class Answer : DomainEntity
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
    /// Short answer summary used for previews and result snippets.
    /// </summary>
    public required string Headline { get; set; }

    /// <summary>
    /// Detailed answer body shown on the thread page or embed.
    /// </summary>
    public string? Body { get; set; }

    public AnswerKind Kind { get; set; } = AnswerKind.Official;
    public AnswerStatus Status { get; set; } = AnswerStatus.Draft;
    public VisibilityScope Visibility { get; set; } = VisibilityScope.PublicIndexed;

    /// <summary>
    /// Language of this answer variant.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Machine-friendly selector for plan/country/version/integration variants.
    /// </summary>
    public string? ContextKey { get; set; }

    /// <summary>
    /// Serialized matching rules for contextual applicability.
    /// </summary>
    public string? ApplicabilityRulesJson { get; set; }

    /// <summary>
    /// Human-readable explanation of why this answer can be trusted.
    /// </summary>
    public string? TrustNote { get; set; }

    /// <summary>
    /// Cached evidence overview for moderation and public trust surfaces.
    /// </summary>
    public string? EvidenceSummary { get; set; }

    /// <summary>
    /// Public author or origin label, such as "Support", "Engineering", or "AI draft".
    /// </summary>
    public string? AuthorLabel { get; set; }

    public int ConfidenceScore { get; set; }
    public int Rank { get; set; }
    public int RevisionNumber { get; set; }

    public bool IsAccepted { get; set; }
    public bool IsCanonical { get; set; } = true;
    public bool IsOfficial { get; set; } = true;

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? ValidatedAtUtc { get; set; }
    public DateTime? AcceptedAtUtc { get; set; }
    public DateTime? RetiredAtUtc { get; set; }

    public ICollection<AnswerSourceLink> Sources { get; set; } = [];
    public ICollection<ThreadActivity> Activity { get; set; } = [];
}
