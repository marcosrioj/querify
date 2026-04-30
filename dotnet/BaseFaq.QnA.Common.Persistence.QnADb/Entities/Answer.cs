using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
///     Represents a candidate, official, or curated answer for a question.
/// </summary>
public class Answer : BaseEntity, IMustHaveTenant
{
    public const int MaxHeadlineLength = 250;
    public const int MaxBodyLength = 6000;
    public const int MaxContextNoteLength = 2000;
    public const int MaxAuthorLabelLength = 200;

    /// <summary>
    ///     Short answer title for previews and quick reading.
    /// </summary>
    public required string Headline { get; set; }

    /// <summary>
    ///     Detailed answer body.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    ///     Functional classification of the answer.
    /// </summary>
    public required AnswerKind Kind { get; set; }

    /// <summary>
    ///     Current workflow state of the answer.
    /// </summary>
    public required AnswerStatus Status { get; set; }

    /// <summary>
    ///     Audience exposure for the answer: internal portal, authenticated external, or public.
    /// </summary>
    public required VisibilityScope Visibility { get; set; }

    /// <summary>
    ///     Human-readable context that explains why or when the answer applies.
    /// </summary>
    public string? ContextNote { get; set; }

    /// <summary>
    ///     Public label for the author or origin of the answer.
    /// </summary>
    public string? AuthorLabel { get; set; }

    /// <summary>
    ///     AI-generated confidence level for the answer.
    /// </summary>
    public required int AiConfidenceScore { get; set; } = 0;

    /// <summary>
    ///     Quality score used to compare answers for the same question.
    /// </summary>
    public required int Score { get; set; }

    /// <summary>
    ///     Manual ordering value used when presenting answers in curated surfaces.
    /// </summary>
    public required int Sort { get; set; }

    /// <summary>
    ///     Id of the question that owns the answer.
    /// </summary>
    public required Guid QuestionId { get; set; }

    /// <summary>
    ///     Question that owns the answer.
    /// </summary>
    public Question Question { get; set; } = null!;

    /// <summary>
    ///     Sources that support the answer.
    /// </summary>
    public ICollection<AnswerSourceLink> Sources { get; set; } = [];

    /// <summary>
    ///     Tenant that owns the answer.
    /// </summary>
    public required Guid TenantId { get; set; }
}
