using System.ComponentModel.DataAnnotations.Schema;
using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Guards;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Top-level container that groups related question threads and defines governance defaults.
/// </summary>
public sealed class QuestionSpace : BaseEntity, IMustHaveTenant
{
    public const int MaxNameLength = 200;
    public const int MaxKeyLength = 160;
    public const int MaxSummaryLength = 2000;
    public const int MaxLanguageLength = 50;
    public const int MaxProductScopeLength = 200;
    public const int MaxJourneyScopeLength = 200;

    private QuestionSpace()
    {
    }

    /// <summary>
    /// Creates a question space with the minimum routing and language information required by the domain.
    /// </summary>
    public QuestionSpace(Guid tenantId, string name, string key, string defaultLanguage, string? createdBy = null)
    {
        Id = Guid.NewGuid();
        TenantId = DomainGuards.AgainstEmpty(tenantId, nameof(tenantId));
        DomainGuards.InitializeAudit(this, createdBy);
        Name = DomainGuards.Required(name, MaxNameLength, nameof(name));
        Key = DomainGuards.Required(key, MaxKeyLength, nameof(key));
        DefaultLanguage = DomainGuards.Required(defaultLanguage, MaxLanguageLength, nameof(defaultLanguage));
    }

    /// <summary>
    /// Tenant boundary that owns the question space and all attached records.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Human-readable name for the public or internal Q&amp;A surface.
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

    /// <summary>
    /// Operating mode of the space, such as curated, community-driven, or hybrid.
    /// </summary>
    public SpaceKind Kind { get; private set; } = SpaceKind.CuratedKnowledge;

    /// <summary>
    /// Audience exposure of the entire space. It stays internal by default.
    /// </summary>
    public VisibilityScope Visibility { get; private set; } = VisibilityScope.Internal;

    /// <summary>
    /// Review posture for new submissions inside the space.
    /// </summary>
    public ModerationPolicy ModerationPolicy { get; private set; } = ModerationPolicy.PreModeration;

    /// <summary>
    /// Search rendering behavior used when the space is exposed on public surfaces.
    /// </summary>
    public SearchMarkupMode SearchMarkupMode { get; private set; } = SearchMarkupMode.Off;

    /// <summary>
    /// Optional product boundary such as Portal, Checkout, or Billing.
    /// </summary>
    public string? ProductScope { get; private set; }

    /// <summary>
    /// Optional journey boundary such as Setup, Troubleshooting, or Renewal.
    /// </summary>
    public string? JourneyScope { get; private set; }

    /// <summary>
    /// Indicates whether new questions can be submitted to the space.
    /// </summary>
    public bool AcceptsQuestions { get; private set; }

    /// <summary>
    /// Indicates whether answer contributions are currently accepted.
    /// </summary>
    public bool AcceptsAnswers { get; private set; }

    /// <summary>
    /// Indicates whether questions must pass review before they are visible.
    /// </summary>
    public bool RequiresQuestionReview { get; private set; } = true;

    /// <summary>
    /// Indicates whether answers must pass review before they are visible.
    /// </summary>
    public bool RequiresAnswerReview { get; private set; } = true;

    /// <summary>
    /// Timestamp that marks when the space first became public.
    /// </summary>
    public DateTime? PublishedAtUtc { get; private set; }

    /// <summary>
    /// Timestamp of the latest governance validation performed on the space.
    /// </summary>
    public DateTime? LastValidatedAtUtc { get; private set; }

    /// <summary>
    /// Primary set of threads that belong to the space.
    /// </summary>
    public ICollection<Question> Questions { get; private set; } = [];

    /// <summary>
    /// Many-to-many persistence links that connect the space to reusable topics.
    /// </summary>
    public ICollection<QuestionSpaceTopic> QuestionSpaceTopics { get; private set; } = [];

    /// <summary>
    /// Many-to-many persistence links that connect the space to curated sources.
    /// </summary>
    public ICollection<QuestionSpaceSource> QuestionSpaceSources { get; private set; } = [];

    /// <summary>
    /// Lightweight taxonomy labels applied to the space.
    /// </summary>
    [NotMapped]
    public IReadOnlyCollection<Topic> Topics => QuestionSpaceTopics.Select(link => link.Topic).ToList();

    /// <summary>
    /// Reusable sources curated at the space level.
    /// </summary>
    [NotMapped]
    public IReadOnlyCollection<KnowledgeSource> CuratedSources => QuestionSpaceSources.Select(link => link.KnowledgeSource).ToList();

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
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
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
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
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
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
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void MarkValidated(string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        LastValidatedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        DomainGuards.Touch(this, updatedBy, LastValidatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void AddQuestion(Question question, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(question);
        DomainGuards.EnsureSameTenant(this, question, "question space membership");
        DomainGuards.Ensure(question.SpaceId == Id, "Question belongs to a different question space.");

        if (Questions.Any(existing => existing.Id == question.Id))
        {
            return;
        }

        Questions.Add(question);
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void AddTopic(Topic topic, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(topic);
        DomainGuards.EnsureSameTenant(this, topic, "question space topic");

        if (QuestionSpaceTopics.Any(existing => existing.TopicId == topic.Id))
        {
            return;
        }

        QuestionSpaceTopics.Add(new QuestionSpaceTopic(this, topic, updatedBy));
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void AddCuratedSource(KnowledgeSource source, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        DomainGuards.EnsureSameTenant(this, source, "question space curated source");

        if (QuestionSpaceSources.Any(existing => existing.KnowledgeSourceId == source.Id))
        {
            return;
        }

        QuestionSpaceSources.Add(new QuestionSpaceSource(this, source, updatedBy));
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }
}
