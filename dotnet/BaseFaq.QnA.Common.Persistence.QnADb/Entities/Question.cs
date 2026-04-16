using System.ComponentModel.DataAnnotations.Schema;
using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Guards;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Represents a single user-facing question thread with lifecycle, routing, and trust metadata.
/// </summary>
public sealed class Question : BaseEntity, IMustHaveTenant
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

    private Question()
    {
    }

    public Question(
        Guid tenantId,
        QuestionSpace space,
        string title,
        string key,
        QuestionKind kind,
        ChannelKind originChannel,
        string? createdBy = null)
    {
        Id = Guid.NewGuid();
        TenantId = DomainGuards.AgainstEmpty(tenantId, nameof(tenantId));
        DomainGuards.InitializeAudit(this, createdBy);
        ArgumentNullException.ThrowIfNull(space);
        DomainGuards.EnsureSameTenant(this, space, "question to question space");

        Title = DomainGuards.Required(title, MaxTitleLength, nameof(title));
        Key = DomainGuards.Required(key, MaxKeyLength, nameof(key));
        Kind = kind;
        OriginChannel = originChannel;
        SpaceId = space.Id;
        Space = space;
    }

    /// <summary>
    /// Tenant boundary that owns the question thread and all attached records.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Main user-facing question.
    /// </summary>
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Stable page and API key for the question thread.
    /// </summary>
    public string Key { get; private set; } = null!;

    /// <summary>
    /// Small search-friendly summary shown in cards and suggestion lists.
    /// </summary>
    public string? Summary { get; private set; }

    /// <summary>
    /// Extra context supplied by the asker, moderator, or ingestion pipeline.
    /// </summary>
    public string? ContextNote { get; private set; }

    /// <summary>
    /// Explains how the question entered the domain.
    /// </summary>
    public QuestionKind Kind { get; private set; } = QuestionKind.Curated;

    /// <summary>
    /// Current workflow state of the thread.
    /// </summary>
    public QuestionStatus Status { get; private set; } = QuestionStatus.Draft;

    /// <summary>
    /// Audience exposure of the thread. It stays internal until explicitly promoted.
    /// </summary>
    public VisibilityScope Visibility { get; private set; } = VisibilityScope.Internal;

    /// <summary>
    /// Channel that created or introduced the question.
    /// </summary>
    public ChannelKind OriginChannel { get; private set; } = ChannelKind.Manual;

    /// <summary>
    /// Locale actually captured from the question source.
    /// </summary>
    public string? Language { get; private set; }

    /// <summary>
    /// Optional product boundary used for routing and segmentation.
    /// </summary>
    public string? ProductScope { get; private set; }

    /// <summary>
    /// Optional journey boundary such as setup, troubleshooting, or renewal.
    /// </summary>
    public string? JourneyScope { get; private set; }

    /// <summary>
    /// Optional audience segment used to distinguish operational variants.
    /// </summary>
    public string? AudienceScope { get; private set; }

    /// <summary>
    /// Machine-friendly grouping key used for plan, country, version, or integration variants.
    /// </summary>
    public string? ContextKey { get; private set; }

    /// <summary>
    /// Absolute origin URL captured from the intake path when available.
    /// </summary>
    public string? OriginUrl { get; private set; }

    /// <summary>
    /// External identifier or free-form reference from the originating system.
    /// </summary>
    public string? OriginReference { get; private set; }

    /// <summary>
    /// Operational summary of what the thread discovered or resolved.
    /// </summary>
    public string? ThreadSummary { get; private set; }

    /// <summary>
    /// Confidence that the thread currently has enough evidence to serve users safely.
    /// </summary>
    public int ConfidenceScore { get; private set; }

    /// <summary>
    /// Current revision pointer. Detailed revision history stays in the activity stream.
    /// </summary>
    public int RevisionNumber { get; private set; }

    /// <summary>
    /// Owning space identifier.
    /// </summary>
    public Guid SpaceId { get; private set; }

    /// <summary>
    /// Owning space navigation.
    /// </summary>
    public QuestionSpace Space { get; private set; } = null!;

    /// <summary>
    /// Identifier of the currently accepted answer, when one exists.
    /// </summary>
    public Guid? AcceptedAnswerId { get; private set; }

    /// <summary>
    /// Accepted answer navigation.
    /// </summary>
    public Answer? AcceptedAnswer { get; private set; }

    /// <summary>
    /// Canonical thread identifier when this question is a duplicate.
    /// </summary>
    public Guid? DuplicateOfQuestionId { get; private set; }

    /// <summary>
    /// Canonical thread navigation when the question is marked as duplicate.
    /// </summary>
    public Question? DuplicateOfQuestion { get; private set; }

    /// <summary>
    /// Timestamp when the thread first received a usable answer.
    /// </summary>
    public DateTime? AnsweredAtUtc { get; private set; }

    /// <summary>
    /// Timestamp when the thread became operationally resolved.
    /// </summary>
    public DateTime? ResolvedAtUtc { get; private set; }

    /// <summary>
    /// Timestamp when the thread passed stronger governance validation.
    /// </summary>
    public DateTime? ValidatedAtUtc { get; private set; }

    /// <summary>
    /// Timestamp of the latest recorded thread event.
    /// </summary>
    public DateTime? LastActivityAtUtc { get; private set; }

    public ICollection<Question> DuplicateQuestions { get; private set; } = [];
    public ICollection<Answer> Answers { get; private set; } = [];
    public ICollection<QuestionSourceLink> Sources { get; private set; } = [];
    public ICollection<QuestionTopic> QuestionTopics { get; private set; } = [];
    public ICollection<ThreadActivity> Activity { get; private set; } = [];

    [NotMapped]
    public IReadOnlyCollection<Topic> Topics => QuestionTopics.Select(link => link.Topic).ToList();

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void UpdateContent(
        string title,
        string key,
        string? summary = null,
        string? contextNote = null,
        string? threadSummary = null,
        string? updatedBy = null,
        DateTime? updatedAtUtc = null)
    {
        Title = DomainGuards.Required(title, MaxTitleLength, nameof(title));
        Key = DomainGuards.Required(key, MaxKeyLength, nameof(key));
        Summary = DomainGuards.Optional(summary, MaxSummaryLength, nameof(summary));
        ContextNote = DomainGuards.Optional(contextNote, MaxContextNoteLength, nameof(contextNote));
        ThreadSummary = DomainGuards.Optional(threadSummary, MaxThreadSummaryLength, nameof(threadSummary));
        RevisionNumber++;
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void SetRouting(
        string? language = null,
        string? productScope = null,
        string? journeyScope = null,
        string? audienceScope = null,
        string? contextKey = null,
        string? updatedBy = null,
        DateTime? updatedAtUtc = null)
    {
        Language = DomainGuards.Optional(language, MaxLanguageLength, nameof(language));
        ProductScope = DomainGuards.Optional(productScope, MaxProductScopeLength, nameof(productScope));
        JourneyScope = DomainGuards.Optional(journeyScope, MaxJourneyScopeLength, nameof(journeyScope));
        AudienceScope = DomainGuards.Optional(audienceScope, MaxAudienceScopeLength, nameof(audienceScope));
        ContextKey = DomainGuards.Optional(contextKey, MaxContextKeyLength, nameof(contextKey));
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void SetOrigin(string? originUrl = null, string? originReference = null, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        OriginUrl = DomainGuards.UriString(originUrl, MaxOriginUrlLength, nameof(originUrl));
        OriginReference = DomainGuards.Optional(originReference, MaxOriginReferenceLength, nameof(originReference));
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void SetTrust(int confidenceScore, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ConfidenceScore = DomainGuards.Range(confidenceScore, 0, 100, nameof(confidenceScore));
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void SetStatus(QuestionStatus status, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        Status = status;

        if (status == QuestionStatus.Validated)
        {
            ValidatedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        }

        EnsureExposureMatchesWorkflow();
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void SetVisibility(VisibilityScope visibility, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        if (visibility.IsPubliclyVisible())
        {
            DomainGuards.Ensure(
                Status is QuestionStatus.Open or QuestionStatus.Answered or QuestionStatus.Validated,
                "Only open, answered, or validated questions can be exposed publicly.");

            foreach (var sourceLink in Sources)
            {
                sourceLink.EnsureCompatibleWithVisibility(visibility);
            }

            if (AcceptedAnswer is not null)
            {
                DomainGuards.Ensure(
                    AcceptedAnswer.Visibility.IsPubliclyVisible(),
                    "Public questions require a publicly visible accepted answer.");
            }
        }

        Visibility = visibility;
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void AddAnswer(Answer answer, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(answer);
        DomainGuards.EnsureSameTenant(this, answer, "question answer");
        DomainGuards.Ensure(answer.QuestionId == Id, "Answer belongs to a different question.");

        if (Answers.Any(existing => existing.Id == answer.Id))
        {
            return;
        }

        Answers.Add(answer);
        LastActivityAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        DomainGuards.Touch(this, updatedBy, LastActivityAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void AcceptAnswer(Answer answer, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(answer);
        DomainGuards.EnsureSameTenant(this, answer, "accepted answer");
        DomainGuards.Ensure(answer.QuestionId == Id, "Accepted answer belongs to a different question.");

        if (Visibility.IsPubliclyVisible())
        {
            DomainGuards.Ensure(
                answer.Visibility.IsPubliclyVisible(),
                "Public questions cannot accept internal-only answers.");
        }

        var resolvedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));

        AcceptedAnswerId = answer.Id;
        AcceptedAnswer = answer;
        AnsweredAtUtc ??= resolvedAtUtc;
        ResolvedAtUtc = resolvedAtUtc;
        Status = Status == QuestionStatus.Validated ? QuestionStatus.Validated : QuestionStatus.Answered;
        LastActivityAtUtc = resolvedAtUtc;
        answer.MarkAccepted(updatedBy, resolvedAtUtc);
        DomainGuards.Touch(this, updatedBy, resolvedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void MarkDuplicateOf(Question canonicalQuestion, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(canonicalQuestion);
        DomainGuards.EnsureSameTenant(this, canonicalQuestion, "duplicate question");
        DomainGuards.Ensure(canonicalQuestion.Id != Id, "Question cannot be marked as duplicate of itself.");

        DuplicateOfQuestionId = canonicalQuestion.Id;
        DuplicateOfQuestion = canonicalQuestion;
        Status = QuestionStatus.Duplicate;

        if (canonicalQuestion.DuplicateQuestions.All(existing => existing.Id != Id))
        {
            canonicalQuestion.DuplicateQuestions.Add(this);
        }

        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void AddSource(QuestionSourceLink sourceLink, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(sourceLink);
        DomainGuards.EnsureSameTenant(this, sourceLink, "question source link");
        DomainGuards.Ensure(sourceLink.QuestionId == Id, "Source link belongs to a different question.");
        sourceLink.EnsureCompatibleWithVisibility(Visibility);

        if (Sources.Any(existing => existing.Id == sourceLink.Id))
        {
            return;
        }

        Sources.Add(sourceLink);
        sourceLink.Source.AttachToQuestion(sourceLink);
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void AddTopic(Topic topic, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(topic);
        DomainGuards.EnsureSameTenant(this, topic, "question topic");

        if (QuestionTopics.Any(existing => existing.TopicId == topic.Id))
        {
            return;
        }

        QuestionTopics.Add(new QuestionTopic(this, topic, updatedBy));
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void AddActivity(ThreadActivity activityEntry, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(activityEntry);
        DomainGuards.EnsureSameTenant(this, activityEntry, "question activity");
        DomainGuards.Ensure(activityEntry.QuestionId == Id, "Activity belongs to a different question.");

        if (Activity.Any(existing => existing.Id == activityEntry.Id))
        {
            return;
        }

        Activity.Add(activityEntry);
        LastActivityAtUtc = activityEntry.OccurredAtUtc;
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    private void EnsureExposureMatchesWorkflow()
    {
        if (!Visibility.IsPubliclyVisible())
        {
            return;
        }

        DomainGuards.Ensure(
            Status is QuestionStatus.Open or QuestionStatus.Answered or QuestionStatus.Validated,
            "Public questions must stay in open, answered, or validated states.");
    }
}
