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

    private readonly List<AnswerSourceLink> sources = [];
    private readonly List<ThreadActivity> activity = [];

    private Answer()
    {
    }

    public Answer(Guid tenantId, Question question, string headline, AnswerKind kind, string? createdBy = null)
        : base(tenantId, createdBy)
    {
        ArgumentNullException.ThrowIfNull(question);
        EnsureSameTenant(question, "answer to question");

        QuestionId = question.Id;
        Question = question;
        Headline = DomainGuards.Required(headline, MaxHeadlineLength, nameof(headline));
        Kind = kind;
    }

    /// <summary>
    /// Short answer summary used for previews and result snippets.
    /// </summary>
    public string Headline { get; private set; } = null!;

    /// <summary>
    /// Detailed answer body shown on the thread page or embed.
    /// </summary>
    public string? Body { get; private set; }

    public AnswerKind Kind { get; private set; }
    public AnswerStatus Status { get; private set; } = AnswerStatus.Draft;
    public VisibilityScope Visibility { get; private set; } = VisibilityScope.Internal;

    /// <summary>
    /// Language of this answer variant.
    /// </summary>
    public string? Language { get; private set; }

    /// <summary>
    /// Machine-friendly selector for plan/country/version/integration variants.
    /// </summary>
    public string? ContextKey { get; private set; }

    /// <summary>
    /// Serialized matching rules for contextual applicability.
    /// </summary>
    public string? ApplicabilityRulesJson { get; private set; }

    /// <summary>
    /// Human-readable explanation of why this answer can be trusted.
    /// </summary>
    public string? TrustNote { get; private set; }

    /// <summary>
    /// Cached evidence overview for moderation and public trust surfaces.
    /// </summary>
    public string? EvidenceSummary { get; private set; }

    /// <summary>
    /// Public author or origin label, such as "Support", "Engineering", or "AI draft".
    /// </summary>
    public string? AuthorLabel { get; private set; }

    public int ConfidenceScore { get; private set; }
    public int Rank { get; private set; }
    public int RevisionNumber { get; private set; }

    public bool IsAccepted { get; private set; }
    public bool IsCanonical { get; private set; }
    public bool IsOfficial { get; private set; }

    public Guid QuestionId { get; private set; }
    public Question Question { get; private set; } = null!;

    public DateTime? PublishedAtUtc { get; private set; }
    public DateTime? ValidatedAtUtc { get; private set; }
    public DateTime? AcceptedAtUtc { get; private set; }
    public DateTime? RetiredAtUtc { get; private set; }

    public IReadOnlyCollection<AnswerSourceLink> Sources => sources;
    public IReadOnlyCollection<ThreadActivity> Activity => activity;

    public void UpdateContent(string headline, string? body = null, string? authorLabel = null, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        Headline = DomainGuards.Required(headline, MaxHeadlineLength, nameof(headline));
        Body = DomainGuards.Optional(body, MaxBodyLength, nameof(body));
        AuthorLabel = DomainGuards.Optional(authorLabel, MaxAuthorLabelLength, nameof(authorLabel));
        Touch(updatedBy, updatedAtUtc);
    }

    public void SetContext(
        string? language = null,
        string? contextKey = null,
        string? applicabilityRulesJson = null,
        string? updatedBy = null,
        DateTime? updatedAtUtc = null)
    {
        Language = DomainGuards.Optional(language, MaxLanguageLength, nameof(language));
        ContextKey = DomainGuards.Optional(contextKey, MaxContextKeyLength, nameof(contextKey));
        ApplicabilityRulesJson = DomainGuards.Json(applicabilityRulesJson, MaxApplicabilityRulesLength, nameof(applicabilityRulesJson));
        Touch(updatedBy, updatedAtUtc);
    }

    public void SetTrust(
        int confidenceScore,
        string? trustNote = null,
        string? evidenceSummary = null,
        string? updatedBy = null,
        DateTime? updatedAtUtc = null)
    {
        ConfidenceScore = DomainGuards.Range(confidenceScore, 0, 100, nameof(confidenceScore));
        TrustNote = DomainGuards.Optional(trustNote, MaxTrustNoteLength, nameof(trustNote));
        EvidenceSummary = DomainGuards.Optional(evidenceSummary, MaxEvidenceSummaryLength, nameof(evidenceSummary));
        Touch(updatedBy, updatedAtUtc);
    }

    public void SetRanking(int rank, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        Rank = DomainGuards.NonNegative(rank, nameof(rank));
        Touch(updatedBy, updatedAtUtc);
    }

    public void SetClassification(
        AnswerKind kind,
        bool isOfficial,
        bool isCanonical,
        string? updatedBy = null,
        DateTime? updatedAtUtc = null)
    {
        Kind = kind;
        IsOfficial = isOfficial;
        IsCanonical = isCanonical;
        Touch(updatedBy, updatedAtUtc);
    }

    public void SetStatus(AnswerStatus status, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        Status = status;
        EnsureExposureMatchesWorkflow();
        Touch(updatedBy, updatedAtUtc);
    }

    public void Publish(string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        var publishedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        Status = AnswerStatus.Published;
        PublishedAtUtc = publishedAtUtc;
        RevisionNumber++;
        Touch(updatedBy, publishedAtUtc);
    }

    public void Validate(string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        var validatedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        Status = AnswerStatus.Validated;
        ValidatedAtUtc = validatedAtUtc;
        RevisionNumber++;
        Touch(updatedBy, validatedAtUtc);
    }

    public void Reject(string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        Status = AnswerStatus.Rejected;
        Visibility = VisibilityScope.Internal;
        Touch(updatedBy, updatedAtUtc);
    }

    public void Retire(string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        var retiredAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        Status = AnswerStatus.Archived;
        Visibility = VisibilityScope.Internal;
        RetiredAtUtc = retiredAtUtc;
        Touch(updatedBy, retiredAtUtc);
    }

    public void SetVisibility(VisibilityScope visibility, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        if (visibility.IsPubliclyVisible())
        {
            DomainGuards.Ensure(
                Status is AnswerStatus.Published or AnswerStatus.Validated,
                "Only published or validated answers can be exposed publicly.");
            DomainGuards.Ensure(
                Kind != AnswerKind.AiDraft || Status == AnswerStatus.Validated,
                "AI draft answers must be validated before public exposure.");

            foreach (var sourceLink in sources)
            {
                sourceLink.EnsureCompatibleWithVisibility(visibility);
            }
        }

        Visibility = visibility;
        Touch(updatedBy, updatedAtUtc);
    }

    public void AddSource(AnswerSourceLink sourceLink, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(sourceLink);
        EnsureSameTenant(sourceLink, "answer source link");
        DomainGuards.Ensure(sourceLink.AnswerId == Id, "Source link belongs to a different answer.");
        sourceLink.EnsureCompatibleWithVisibility(Visibility);

        if (sources.Any(existing => existing.Id == sourceLink.Id))
        {
            return;
        }

        sources.Add(sourceLink);
        sourceLink.Source.AttachToAnswer(sourceLink);
        Touch(updatedBy, updatedAtUtc);
    }

    public void AddActivity(ThreadActivity activityEntry, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(activityEntry);
        EnsureSameTenant(activityEntry, "answer activity");
        DomainGuards.Ensure(activityEntry.QuestionId == QuestionId, "Activity belongs to a different question.");
        DomainGuards.Ensure(activityEntry.AnswerId == Id, "Activity belongs to a different answer.");

        if (activity.Any(existing => existing.Id == activityEntry.Id))
        {
            return;
        }

        activity.Add(activityEntry);
        Touch(updatedBy, updatedAtUtc);
    }

    internal void MarkAccepted(string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        var acceptedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        DomainGuards.Ensure(
            Status is AnswerStatus.Published or AnswerStatus.Validated,
            "Only published or validated answers can be accepted.");

        IsAccepted = true;
        AcceptedAtUtc = acceptedAtUtc;
        Touch(updatedBy, acceptedAtUtc);
    }

    private void EnsureExposureMatchesWorkflow()
    {
        if (!Visibility.IsPubliclyVisible())
        {
            return;
        }

        DomainGuards.Ensure(
            Status is AnswerStatus.Published or AnswerStatus.Validated,
            "Public answers must stay in published or validated workflow states.");
    }
}
