using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Guards;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Stores an answer candidate or answer variant attached to a question thread.
/// </summary>
public sealed class Answer : BaseEntity, IMustHaveTenant
{
    public const int MaxHeadlineLength = 250;
    public const int MaxBodyLength = 6000;
    public const int MaxLanguageLength = 50;
    public const int MaxContextKeyLength = 200;
    public const int MaxApplicabilityRulesLength = 4000;
    public const int MaxTrustNoteLength = 2000;
    public const int MaxEvidenceSummaryLength = 4000;
    public const int MaxAuthorLabelLength = 200;

    private Answer()
    {
    }

    public Answer(Guid tenantId, Question question, string headline, AnswerKind kind, string? createdBy = null)
    {
        Id = Guid.NewGuid();
        TenantId = DomainGuards.AgainstEmpty(tenantId, nameof(tenantId));
        DomainGuards.InitializeAudit(this, createdBy);
        ArgumentNullException.ThrowIfNull(question);
        DomainGuards.EnsureSameTenant(this, question, "answer to question");

        QuestionId = question.Id;
        Question = question;
        Headline = DomainGuards.Required(headline, MaxHeadlineLength, nameof(headline));
        Kind = kind;
    }

    /// <summary>
    /// Tenant boundary that owns the answer candidate and all attached evidence.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Short answer summary used for previews and result snippets.
    /// </summary>
    public string Headline { get; private set; } = null!;

    /// <summary>
    /// Detailed answer body shown on the thread page or embed.
    /// </summary>
    public string? Body { get; private set; }

    /// <summary>
    /// Explains where the answer came from.
    /// </summary>
    public AnswerKind Kind { get; private set; } = AnswerKind.Official;

    /// <summary>
    /// Current lifecycle state of the answer candidate.
    /// </summary>
    public AnswerStatus Status { get; private set; } = AnswerStatus.Draft;

    /// <summary>
    /// Audience exposure of the answer. It stays internal by default.
    /// </summary>
    public VisibilityScope Visibility { get; private set; } = VisibilityScope.Internal;

    /// <summary>
    /// Language of this answer variant.
    /// </summary>
    public string? Language { get; private set; }

    /// <summary>
    /// Machine-friendly selector for plan, country, version, or integration variants.
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
    /// Public author or origin label, such as Support, Engineering, or AI draft.
    /// </summary>
    public string? AuthorLabel { get; private set; }

    /// <summary>
    /// Confidence level for this answer itself.
    /// </summary>
    public int ConfidenceScore { get; private set; }

    /// <summary>
    /// Ordering signal among answer candidates.
    /// </summary>
    public int Rank { get; private set; }

    /// <summary>
    /// Revision pointer for the answer variant.
    /// </summary>
    public int RevisionNumber { get; private set; }

    /// <summary>
    /// Indicates whether the answer is the chosen resolution of the thread.
    /// </summary>
    public bool IsAccepted { get; private set; }

    /// <summary>
    /// Indicates whether the answer is the preferred canonical variant.
    /// </summary>
    public bool IsCanonical { get; private set; }

    /// <summary>
    /// Indicates whether the answer is officially owned by the operation.
    /// </summary>
    public bool IsOfficial { get; private set; }

    /// <summary>
    /// Owning question identifier.
    /// </summary>
    public Guid QuestionId { get; private set; }

    /// <summary>
    /// Owning question navigation.
    /// </summary>
    public Question Question { get; private set; } = null!;

    /// <summary>
    /// Timestamp when the answer became visible for use.
    /// </summary>
    public DateTime? PublishedAtUtc { get; private set; }

    /// <summary>
    /// Timestamp when the answer passed stronger validation.
    /// </summary>
    public DateTime? ValidatedAtUtc { get; private set; }

    /// <summary>
    /// Timestamp when the answer was accepted by the question thread.
    /// </summary>
    public DateTime? AcceptedAtUtc { get; private set; }

    /// <summary>
    /// Timestamp when the answer was retired from active use.
    /// </summary>
    public DateTime? RetiredAtUtc { get; private set; }

    /// <summary>
    /// Evidence and citation links attached to the answer.
    /// </summary>
    public ICollection<AnswerSourceLink> Sources { get; private set; } = [];

    /// <summary>
    /// Journal entries that target the answer inside the thread timeline.
    /// </summary>
    public ICollection<ThreadActivity> Activity { get; private set; } = [];

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void UpdateContent(string headline, string? body = null, string? authorLabel = null, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        Headline = DomainGuards.Required(headline, MaxHeadlineLength, nameof(headline));
        Body = DomainGuards.Optional(body, MaxBodyLength, nameof(body));
        AuthorLabel = DomainGuards.Optional(authorLabel, MaxAuthorLabelLength, nameof(authorLabel));
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
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
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
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
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void SetRanking(int rank, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        Rank = DomainGuards.NonNegative(rank, nameof(rank));
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
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
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void SetStatus(AnswerStatus status, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        Status = status;
        EnsureExposureMatchesWorkflow();
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void Publish(string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        var publishedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        Status = AnswerStatus.Published;
        PublishedAtUtc = publishedAtUtc;
        RevisionNumber++;
        DomainGuards.Touch(this, updatedBy, publishedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void Validate(string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        var validatedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        Status = AnswerStatus.Validated;
        ValidatedAtUtc = validatedAtUtc;
        RevisionNumber++;
        DomainGuards.Touch(this, updatedBy, validatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void Reject(string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        Status = AnswerStatus.Rejected;
        Visibility = VisibilityScope.Internal;
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void Retire(string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        var retiredAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        Status = AnswerStatus.Archived;
        Visibility = VisibilityScope.Internal;
        RetiredAtUtc = retiredAtUtc;
        DomainGuards.Touch(this, updatedBy, retiredAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
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

            foreach (var sourceLink in Sources)
            {
                sourceLink.EnsureCompatibleWithVisibility(visibility);
            }
        }

        Visibility = visibility;
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void AddSource(AnswerSourceLink sourceLink, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(sourceLink);
        DomainGuards.EnsureSameTenant(this, sourceLink, "answer source link");
        DomainGuards.Ensure(sourceLink.AnswerId == Id, "Source link belongs to a different answer.");
        sourceLink.EnsureCompatibleWithVisibility(Visibility);

        if (Sources.Any(existing => existing.Id == sourceLink.Id))
        {
            return;
        }

        Sources.Add(sourceLink);
        sourceLink.Source.AttachToAnswer(sourceLink);
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void AddActivity(ThreadActivity activityEntry, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        ArgumentNullException.ThrowIfNull(activityEntry);
        DomainGuards.EnsureSameTenant(this, activityEntry, "answer activity");
        DomainGuards.Ensure(activityEntry.QuestionId == QuestionId, "Activity belongs to a different question.");
        DomainGuards.Ensure(activityEntry.AnswerId == Id, "Activity belongs to a different answer.");

        if (Activity.Any(existing => existing.Id == activityEntry.Id))
        {
            return;
        }

        Activity.Add(activityEntry);
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    internal void MarkAccepted(string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        var acceptedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));
        DomainGuards.Ensure(
            Status is AnswerStatus.Published or AnswerStatus.Validated,
            "Only published or validated answers can be accepted.");

        IsAccepted = true;
        AcceptedAtUtc = acceptedAtUtc;
        DomainGuards.Touch(this, updatedBy, acceptedAtUtc);
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
