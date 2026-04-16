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

    private readonly List<Answer> answers = [];
    private readonly List<QuestionSourceLink> sources = [];
    private readonly List<Tag> tags = [];
    private readonly List<ThreadActivity> activity = [];
    private readonly List<Question> duplicateQuestions = [];

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
        : base(tenantId, createdBy)
    {
        ArgumentNullException.ThrowIfNull(space);
        EnsureSameTenant(space, "question to question space");

        Title = DomainGuards.Required(title, MaxTitleLength, nameof(title));
        Key = DomainGuards.Required(key, MaxKeyLength, nameof(key));
        Kind = kind;
        OriginChannel = originChannel;
        SpaceId = space.Id;
        Space = space;
    }

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

    public QuestionKind Kind { get; private set; }
    public QuestionStatus Status { get; private set; } = QuestionStatus.Draft;
    public VisibilityScope Visibility { get; private set; } = VisibilityScope.Internal;
    public ChannelKind OriginChannel { get; private set; }

    /// <summary>
    /// Locale actually captured from the question source.
    /// </summary>
    public string? Language { get; private set; }

    public string? ProductScope { get; private set; }
    public string? JourneyScope { get; private set; }
    public string? AudienceScope { get; private set; }

    /// <summary>
    /// Machine-friendly grouping key used for plan/country/version variants.
    /// </summary>
    public string? ContextKey { get; private set; }

    public string? OriginUrl { get; private set; }
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
    /// Current revision pointer. The detailed history stays in the append-only activity stream.
    /// </summary>
    public int RevisionNumber { get; private set; }

    public Guid SpaceId { get; private set; }
    public QuestionSpace Space { get; private set; } = null!;

    public Guid? AcceptedAnswerId { get; private set; }
    public Answer? AcceptedAnswer { get; private set; }

    public Guid? DuplicateOfQuestionId { get; private set; }
    public Question? DuplicateOfQuestion { get; private set; }
    public IReadOnlyCollection<Question> DuplicateQuestions => duplicateQuestions;

    public DateTime? AnsweredAtUtc { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }
    public DateTime? ValidatedAtUtc { get; private set; }
    public DateTime? LastActivityAtUtc { get; private set; }

    public IReadOnlyCollection<Answer> Answers => answers;
    public IReadOnlyCollection<QuestionSourceLink> Sources => sources;
    public IReadOnlyCollection<Tag> Tags => tags;
    public IReadOnlyCollection<ThreadActivity> Activity => activity;

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
        Touch(updatedBy, updatedAtUtc);
    }

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
        Touch(updatedBy, updatedAtUtc);
    }

    public void SetOrigin(string? originUrl = null, string? originReference = null, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        OriginUrl = DomainGuards.UriString(originUrl, MaxOriginUrlLength, nameof(originUrl));
        OriginReference = DomainGuards.Optional(originReference, MaxOriginReferenceLength, nameof(originReference));
        Touch(updatedBy, updatedAtUtc);
    }

    public void SetTrust(int confidenceScore, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ConfidenceScore = DomainGuards.Range(confidenceScore, 0, 100, nameof(confidenceScore));
        Touch(updatedBy, updatedAtUtc);
    }

    public void SetStatus(QuestionStatus status, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        Status = status;

        if (status == QuestionStatus.Validated)
        {
            ValidatedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        }

        EnsureExposureMatchesWorkflow();
        Touch(updatedBy, updatedAtUtc);
    }

    public void SetVisibility(VisibilityScope visibility, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        if (visibility.IsPubliclyVisible())
        {
            DomainGuards.Ensure(
                Status is QuestionStatus.Open or QuestionStatus.Answered or QuestionStatus.Validated,
                "Only open, answered, or validated questions can be exposed publicly.");

            foreach (var sourceLink in sources)
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
        Touch(updatedBy, updatedAtUtc);
    }

    public void AddAnswer(Answer answer, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(answer);
        EnsureSameTenant(answer, "question answer");
        DomainGuards.Ensure(answer.QuestionId == Id, "Answer belongs to a different question.");

        if (answers.Any(existing => existing.Id == answer.Id))
        {
            return;
        }

        answers.Add(answer);
        LastActivityAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        Touch(updatedBy, LastActivityAtUtc);
    }

    public void AcceptAnswer(Answer answer, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(answer);
        EnsureSameTenant(answer, "accepted answer");
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
        Touch(updatedBy, resolvedAtUtc);
    }

    public void MarkDuplicateOf(Question canonicalQuestion, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(canonicalQuestion);
        EnsureSameTenant(canonicalQuestion, "duplicate question");
        DomainGuards.Ensure(canonicalQuestion.Id != Id, "Question cannot be marked as duplicate of itself.");

        DuplicateOfQuestionId = canonicalQuestion.Id;
        DuplicateOfQuestion = canonicalQuestion;
        Status = QuestionStatus.Duplicate;

        if (canonicalQuestion.duplicateQuestions.All(existing => existing.Id != Id))
        {
            canonicalQuestion.duplicateQuestions.Add(this);
        }

        Touch(updatedBy, updatedAtUtc);
    }

    public void AddSource(QuestionSourceLink sourceLink, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(sourceLink);
        EnsureSameTenant(sourceLink, "question source link");
        DomainGuards.Ensure(sourceLink.QuestionId == Id, "Source link belongs to a different question.");
        sourceLink.EnsureCompatibleWithVisibility(Visibility);

        if (sources.Any(existing => existing.Id == sourceLink.Id))
        {
            return;
        }

        sources.Add(sourceLink);
        sourceLink.Source.AttachToQuestion(sourceLink);
        Touch(updatedBy, updatedAtUtc);
    }

    public void AddTag(Tag tag, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(tag);
        EnsureSameTenant(tag, "question tag");

        if (tags.Any(existing => existing.Id == tag.Id))
        {
            return;
        }

        tags.Add(tag);
        tag.AttachToQuestion(this);
        Touch(updatedBy, updatedAtUtc);
    }

    public void AddActivity(ThreadActivity activityEntry, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(activityEntry);
        EnsureSameTenant(activityEntry, "question activity");
        DomainGuards.Ensure(activityEntry.QuestionId == Id, "Activity belongs to a different question.");

        if (activity.Any(existing => existing.Id == activityEntry.Id))
        {
            return;
        }

        activity.Add(activityEntry);
        LastActivityAtUtc = activityEntry.OccurredAtUtc;
        Touch(updatedBy, updatedAtUtc);
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
