using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
///     Represents the main thread for a question inside a Q&amp;A space.
/// </summary>
public class Question : BaseEntity, IMustHaveTenant
{
    public const int MaxTitleLength = 1000;
    public const int MaxSummaryLength = 500;
    public const int MaxContextNoteLength = 2000;

    /// <summary>
    ///     Main question shown to the user.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    ///     Short question summary for lists, search, and suggestions.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    ///     Additional context provided by the user, moderation, or ingestion.
    /// </summary>
    public string? ContextNote { get; set; }

    /// <summary>
    ///     Current workflow state of the question.
    /// </summary>
    public required QuestionStatus Status { get; set; }

    /// <summary>
    ///     Visibility scope of the question.
    /// </summary>
    public required VisibilityScope Visibility { get; set; }

    /// <summary>
    ///     Channel where the question originated.
    /// </summary>
    public required ChannelKind OriginChannel { get; set; }

    /// <summary>
    ///     AI-generated confidence that the question is well answered and safe to serve.
    /// </summary>
    public required int AiConfidenceScore { get; set; } = 0;

    /// <summary>
    ///     Current aggregate feedback score from public feedback signals.
    /// </summary>
    public required int FeedbackScore { get; set; }

    /// <summary>
    ///     Manual ordering value used when presenting questions in curated surfaces.
    /// </summary>
    public required int Sort { get; set; }

    /// <summary>
    ///     Identifier of the space that owns the question.
    /// </summary>
    public required Guid SpaceId { get; set; }

    /// <summary>
    ///     Space that owns the question.
    /// </summary>
    public Space Space { get; set; } = null!;

    /// <summary>
    ///     Id of the answer accepted as the primary answer for the question.
    /// </summary>
    public Guid? AcceptedAnswerId { get; set; }

    /// <summary>
    ///     Answer accepted as the primary answer for the question.
    /// </summary>
    public Answer? AcceptedAnswer { get; set; }

    /// <summary>
    ///     Id of the canonical question when this question is a duplicate.
    /// </summary>
    public Guid? DuplicateOfQuestionId { get; set; }

    /// <summary>
    ///     Canonical question when this thread was marked as a duplicate.
    /// </summary>
    public Question? DuplicateOfQuestion { get; set; }

    /// <summary>
    ///     Timestamp when the question received its primary answer.
    /// </summary>
    public DateTime? AnsweredAtUtc { get; set; }

    /// <summary>
    ///     Timestamp when the question was validated.
    /// </summary>
    public DateTime? ValidatedAtUtc { get; set; }

    /// <summary>
    ///     Timestamp of the last relevant activity in the thread.
    /// </summary>
    public DateTime? LastActivityAtUtc { get; set; }

    /// <summary>
    ///     Questions that point to this one as their duplicate target.
    /// </summary>
    public ICollection<Question> DuplicateQuestions { get; set; } = [];

    /// <summary>
    ///     Answers associated with the question.
    /// </summary>
    public ICollection<Answer> Answers { get; set; } = [];

    /// <summary>
    ///     Sources linked directly to the question.
    /// </summary>
    public ICollection<QuestionSourceLink> Sources { get; set; } = [];

    /// <summary>
    ///     Relationships between the question and its tags.
    /// </summary>
    public ICollection<QuestionTag> Tags { get; set; } = [];

    /// <summary>
    ///     History of thread events and changes.
    /// </summary>
    public ICollection<Activity> Activities { get; set; } = [];

    /// <summary>
    ///     Tenant that owns the question.
    /// </summary>
    public required Guid TenantId { get; set; }
}
