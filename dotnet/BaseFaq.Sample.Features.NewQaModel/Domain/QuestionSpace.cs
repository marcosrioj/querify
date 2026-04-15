using BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class QuestionSpace : DomainEntity
{
    public const int MaxNameLength = 200;
    public const int MaxKeyLength = 160;
    public const int MaxSummaryLength = 2000;
    public const int MaxLanguageLength = 50;
    public const int MaxProductScopeLength = 200;
    public const int MaxJourneyScopeLength = 200;

    /// <summary>
    /// Human-readable name for the public or internal Q&A surface.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Stable routing and API key used by embeds, public pages, and integrations.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Description that explains what kind of questions belong in this space.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Default locale used for curation, search, and rendering.
    /// </summary>
    public required string DefaultLanguage { get; set; }

    public SpaceKind Kind { get; set; } = SpaceKind.CuratedKnowledge;
    public VisibilityScope Visibility { get; set; } = VisibilityScope.PublicIndexed;
    public ModerationPolicy ModerationPolicy { get; set; } = ModerationPolicy.PreModeration;
    public SearchMarkupMode SearchMarkupMode { get; set; } = SearchMarkupMode.CuratedList;

    /// <summary>
    /// Optional product boundary such as "Portal", "Checkout", or "Billing".
    /// </summary>
    public string? ProductScope { get; set; }

    /// <summary>
    /// Optional journey boundary such as "Setup", "Troubleshooting", or "Renewal".
    /// </summary>
    public string? JourneyScope { get; set; }

    public bool AcceptsQuestions { get; set; } = true;
    public bool AcceptsAnswers { get; set; }
    public bool RequiresQuestionReview { get; set; } = true;
    public bool RequiresAnswerReview { get; set; } = true;

    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? LastValidatedAtUtc { get; set; }

    public ICollection<Question> Questions { get; set; } = [];
    public ICollection<Topic> Topics { get; set; } = [];
    public ICollection<KnowledgeSource> CuratedSources { get; set; } = [];
}
