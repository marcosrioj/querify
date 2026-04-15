using BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class Question : DomainEntity
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
    /// Main user-facing question.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Stable page and API key for the question thread.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Small search-friendly summary shown in cards and suggestion lists.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Extra context supplied by the asker, moderator, or ingestion pipeline.
    /// </summary>
    public string? ContextNote { get; set; }

    public QuestionKind Kind { get; set; } = QuestionKind.Curated;
    public QuestionStatus Status { get; set; } = QuestionStatus.Draft;
    public VisibilityScope Visibility { get; set; } = VisibilityScope.PublicIndexed;
    public ChannelKind OriginChannel { get; set; } = ChannelKind.Manual;

    /// <summary>
    /// Locale actually captured from the question source.
    /// </summary>
    public string? Language { get; set; }

    public string? ProductScope { get; set; }
    public string? JourneyScope { get; set; }
    public string? AudienceScope { get; set; }

    /// <summary>
    /// Machine-friendly grouping key used for plan/country/version variants.
    /// </summary>
    public string? ContextKey { get; set; }

    public string? OriginUrl { get; set; }
    public string? OriginReference { get; set; }

    /// <summary>
    /// Operational summary of what the thread discovered or resolved.
    /// </summary>
    public string? ThreadSummary { get; set; }

    /// <summary>
    /// Confidence that the thread currently has enough evidence to serve users safely.
    /// </summary>
    public int ConfidenceScore { get; set; }

    /// <summary>
    /// Current revision pointer. The detailed history stays in the append-only activity stream.
    /// </summary>
    public int RevisionNumber { get; set; }

    public Guid SpaceId { get; set; }
    public QuestionSpace Space { get; set; } = null!;

    public Guid? AcceptedAnswerId { get; set; }
    public Answer? AcceptedAnswer { get; set; }

    public Guid? DuplicateOfQuestionId { get; set; }
    public Question? DuplicateOfQuestion { get; set; }
    public ICollection<Question> DuplicateQuestions { get; set; } = [];

    public DateTime? AnsweredAtUtc { get; set; }
    public DateTime? ResolvedAtUtc { get; set; }
    public DateTime? ValidatedAtUtc { get; set; }
    public DateTime? LastActivityAtUtc { get; set; }

    public ICollection<Answer> Answers { get; set; } = [];
    public ICollection<QuestionSourceLink> Sources { get; set; } = [];
    public ICollection<Topic> Topics { get; set; } = [];
    public ICollection<ThreadActivity> Activity { get; set; } = [];
}
