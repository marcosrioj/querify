using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Represents the main thread for a question inside a Q&amp;A space.
/// </summary>
public class Question : BaseEntity, IMustHaveTenant
{
    public const int MaxTitleLength = 1000;
    public const int MaxKeyLength = 200;
    public const int MaxSummaryLength = 500;
    public const int MaxContextNoteLength = 2000;
    public const int MaxLanguageLength = 50;
    public const int MaxProductScopeLength = 200;
    public const int MaxJourneyScopeLength = 200;
    public const int MaxAudienceScopeLength = 200;
    public const int MaxContextKeyLength = 200;
    public const int MaxOriginUrlLength = 1000;
    public const int MaxOriginReferenceLength = 250;
    public const int MaxThreadSummaryLength = 4000;

    /// <summary>
    /// Main question shown to the user.
    /// </summary>
    public required string Title { get; set; } = null!;

    /// <summary>
    /// Stable question key for pages, APIs, and integrations.
    /// </summary>
    public required string Key { get; set; } = null!;

    /// <summary>
    /// Short question summary for lists, search, and suggestions.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Additional context provided by the user, moderation, or ingestion.
    /// </summary>
    public string? ContextNote { get; set; }

    /// <summary>
    /// Functional classification of the question.
    /// </summary>
    public QuestionKind Kind { get; set; } = QuestionKind.Curated;

    /// <summary>
    /// Current workflow state of the question.
    /// </summary>
    public QuestionStatus Status { get; set; } = QuestionStatus.Draft;

    /// <summary>
    /// Visibility scope of the question.
    /// </summary>
    public VisibilityScope Visibility { get; set; } = VisibilityScope.Internal;

    /// <summary>
    /// Channel where the question originated.
    /// </summary>
    public ChannelKind OriginChannel { get; set; } = ChannelKind.Manual;

    /// <summary>
    /// Language captured for the question.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Optional product scope associated with the question.
    /// </summary>
    public string? ProductScope { get; set; }

    /// <summary>
    /// Optional journey scope associated with the question.
    /// </summary>
    public string? JourneyScope { get; set; }

    /// <summary>
    /// Optional audience scope that the question applies to.
    /// </summary>
    public string? AudienceScope { get; set; }

    /// <summary>
    /// Context key for plan, country, version, or integration variants.
    /// </summary>
    public string? ContextKey { get; set; }

    /// <summary>
    /// Source URL for the question when it comes from another channel.
    /// </summary>
    public string? OriginUrl { get; set; }

    /// <summary>
    /// Source reference such as a ticket, message, or external identifier.
    /// </summary>
    public string? OriginReference { get; set; }

    /// <summary>
    /// Operational summary of what the thread discovered or resolved.
    /// </summary>
    public string? ThreadSummary { get; set; }

    /// <summary>
    /// Accumulated confidence that the question is well answered and safe to serve.
    /// </summary>
    public int ConfidenceScore { get; set; }

    /// <summary>
    /// Current revision of the thread.
    /// </summary>
    public int RevisionNumber { get; set; }

    /// <summary>
    /// Identifier of the space that owns the question.
    /// </summary>
    public required Guid SpaceId { get; set; }

    /// <summary>
    /// Space that owns the question.
    /// </summary>
    public QuestionSpace Space { get; set; } = null!;

    /// <summary>
    /// Id of the answer accepted as the primary answer for the question.
    /// </summary>
    public Guid? AcceptedAnswerId { get; set; }

    /// <summary>
    /// Answer accepted as the primary answer for the question.
    /// </summary>
    public Answer? AcceptedAnswer { get; set; }

    /// <summary>
    /// Id of the canonical question when this question is a duplicate.
    /// </summary>
    public Guid? DuplicateOfQuestionId { get; set; }

    /// <summary>
    /// Canonical question when this thread was marked as a duplicate.
    /// </summary>
    public Question? DuplicateOfQuestion { get; set; }

    /// <summary>
    /// Timestamp when the question received its primary answer.
    /// </summary>
    public DateTime? AnsweredAtUtc { get; set; }

    /// <summary>
    /// Timestamp when the question was considered resolved.
    /// </summary>
    public DateTime? ResolvedAtUtc { get; set; }

    /// <summary>
    /// Timestamp when the question was validated.
    /// </summary>
    public DateTime? ValidatedAtUtc { get; set; }

    /// <summary>
    /// Timestamp of the last relevant activity in the thread.
    /// </summary>
    public DateTime? LastActivityAtUtc { get; set; }

    /// <summary>
    /// Questions that point to this one as their duplicate target.
    /// </summary>
    public ICollection<Question> DuplicateQuestions { get; set; } = [];

    /// <summary>
    /// Answers associated with the question.
    /// </summary>
    public ICollection<Answer> Answers { get; set; } = [];

    /// <summary>
    /// Sources linked directly to the question.
    /// </summary>
    public ICollection<QuestionSourceLink> Sources { get; set; } = [];

    /// <summary>
    /// Relationships between the question and its tags.
    /// </summary>
    public ICollection<QuestionTag> Tags { get; set; } = [];

    /// <summary>
    /// History of thread events and changes.
    /// </summary>
    public ICollection<ThreadActivity> Activities { get; set; } = [];

    /// <summary>
    /// Tenant that owns the question.
    /// </summary>
    public required Guid TenantId { get; set; }
}
