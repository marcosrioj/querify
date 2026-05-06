using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.EntityFramework.Core.Entities;
using Querify.Models.QnA.Enums;

namespace Querify.QnA.Common.Domain.Entities;

/// <summary>
///     Represents a canonical question inside a Q&amp;A space.
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
    ///     Audience exposure for the question: internal portal, authenticated external, or public.
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
    ///     Timestamp of the last relevant activity for the question.
    /// </summary>
    public DateTime? LastActivityAtUtc { get; set; }

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
    ///     History of question events and changes.
    /// </summary>
    public ICollection<Activity> Activities { get; set; } = [];

    /// <summary>
    ///     Tenant that owns the question.
    /// </summary>
    public required Guid TenantId { get; set; }
}
