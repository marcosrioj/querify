using BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class Space : DomainEntity
{
    public const int MaxNameLength = 200;
    public const int MaxKeyLength = 160;
    public const int MaxSummaryLength = 2000;
    public const int MaxLanguageLength = 50;
    public const int MaxProductScopeLength = 200;
    public const int MaxJourneyScopeLength = 200;

    private readonly List<Question> questions = [];
    private readonly List<Tag> tags = [];
    private readonly List<Source> curatedSources = [];

    private Space()
    {
    }

    public Space(Guid tenantId, string name, string key, string defaultLanguage, string? createdBy = null)
        : base(tenantId, createdBy)
    {
        Name = DomainGuards.Required(name, MaxNameLength, nameof(name));
        Key = DomainGuards.Required(key, MaxKeyLength, nameof(key));
        DefaultLanguage = DomainGuards.Required(defaultLanguage, MaxLanguageLength, nameof(defaultLanguage));
    }

    /// <summary>
    /// Human-readable name for the public or internal Q&A surface.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Stable routing and API key used by embeds, public pages, and integrations.
    /// </summary>
    public string Key { get; private set; } = null!;

    /// <summary>
    /// Description that explains what kind of questions belong in this space.
    /// </summary>
    public string? Summary { get; private set; }

    /// <summary>
    /// Default locale used for curation, search, and rendering.
    /// </summary>
    public string DefaultLanguage { get; private set; } = null!;

    public SpaceKind Kind { get; set; } = SpaceKind.CuratedKnowledge;
    public VisibilityScope Visibility { get; private set; } = VisibilityScope.Internal;
    public ModerationPolicy ModerationPolicy { get; private set; } = ModerationPolicy.PreModeration;
    public SearchMarkupMode SearchMarkupMode { get; private set; } = SearchMarkupMode.Off;

    /// <summary>
    /// Optional product boundary such as "Portal", "Checkout", or "Billing".
    /// </summary>
    public string? ProductScope { get; private set; }

    /// <summary>
    /// Optional journey boundary such as "Setup", "Troubleshooting", or "Renewal".
    /// </summary>
    public string? JourneyScope { get; private set; }

    public bool AcceptsQuestions { get; private set; }
    public bool AcceptsAnswers { get; private set; }
    public bool RequiresQuestionReview { get; private set; } = true;
    public bool RequiresAnswerReview { get; private set; } = true;

    public DateTime? PublishedAtUtc { get; private set; }
    public DateTime? LastValidatedAtUtc { get; private set; }

    public IReadOnlyCollection<Question> Questions => questions;
    public IReadOnlyCollection<Tag> Tags => tags;
    public IReadOnlyCollection<Source> CuratedSources => curatedSources;

    public void UpdateMetadata(
        string name,
        string key,
        string defaultLanguage,
        string? summary = null,
        string? productScope = null,
        string? journeyScope = null,
        string? updatedBy = null,
        DateTime? updatedAtUtc = null)
    {
        Name = DomainGuards.Required(name, MaxNameLength, nameof(name));
        Key = DomainGuards.Required(key, MaxKeyLength, nameof(key));
        DefaultLanguage = DomainGuards.Required(defaultLanguage, MaxLanguageLength, nameof(defaultLanguage));
        Summary = DomainGuards.Optional(summary, MaxSummaryLength, nameof(summary));
        ProductScope = DomainGuards.Optional(productScope, MaxProductScopeLength, nameof(productScope));
        JourneyScope = DomainGuards.Optional(journeyScope, MaxJourneyScopeLength, nameof(journeyScope));
        Touch(updatedBy, updatedAtUtc);
    }

    public void ConfigureGovernance(
        ModerationPolicy moderationPolicy,
        bool acceptsQuestions,
        bool acceptsAnswers,
        bool requiresQuestionReview,
        bool requiresAnswerReview,
        string? updatedBy = null,
        DateTime? updatedAtUtc = null)
    {
        ModerationPolicy = moderationPolicy;
        AcceptsQuestions = acceptsQuestions;
        AcceptsAnswers = acceptsAnswers;
        RequiresQuestionReview = requiresQuestionReview;
        RequiresAnswerReview = requiresAnswerReview;
        Touch(updatedBy, updatedAtUtc);
    }

    public void ConfigureExposure(
        VisibilityScope visibility,
        SearchMarkupMode searchMarkupMode,
        string? updatedBy = null,
        DateTime? updatedAtUtc = null)
    {
        if (visibility.IsPubliclyVisible())
        {
            PublishedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        }
        else
        {
            PublishedAtUtc = null;
        }

        Visibility = visibility;
        SearchMarkupMode = searchMarkupMode;
        Touch(updatedBy, updatedAtUtc);
    }

    public void MarkValidated(string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        LastValidatedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        Touch(updatedBy, LastValidatedAtUtc);
    }

    public void AddQuestion(Question question, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(question);
        EnsureSameTenant(question, "space membership");
        DomainGuards.Ensure(question.SpaceId == Id, "Question belongs to a different space.");

        if (questions.Any(existing => existing.Id == question.Id))
        {
            return;
        }

        questions.Add(question);
        Touch(updatedBy, updatedAtUtc);
    }

    public void AddTag(Tag tag, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(tag);
        EnsureSameTenant(tag, "space tag");

        if (tags.Any(existing => existing.Id == tag.Id))
        {
            return;
        }

        tags.Add(tag);
        tag.AttachToSpace(this);
        Touch(updatedBy, updatedAtUtc);
    }

    public void AddCuratedSource(Source source, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        EnsureSameTenant(source, "space curated source");

        if (curatedSources.Any(existing => existing.Id == source.Id))
        {
            return;
        }

        curatedSources.Add(source);
        source.AttachToSpace(this);
        Touch(updatedBy, updatedAtUtc);
    }
}
