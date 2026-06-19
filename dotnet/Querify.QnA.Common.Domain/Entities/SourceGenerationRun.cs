using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.EntityFramework.Core.Entities;
using Querify.Models.QnA.Enums;

namespace Querify.QnA.Common.Domain.Entities;

/// <summary>
///     Durable status for a source-to-space generation request.
/// </summary>
public class SourceGenerationRun : BaseEntity, IMustHaveTenant
{
    public const int MaxSpaceNameLength = 200;
    public const int MaxSpaceSlugLength = 160;
    public const int MaxLanguageLength = 50;
    public const int MaxExtractionGoalLength = 2000;
    public const int MaxContentHintLength = 2000;
    public const int MaxFailureReasonLength = 2000;
    public const int MaxWarningLength = 2000;
    public const int MaxRawOutputJsonLength = 12000;

    /// <summary>
    ///     Source used as the grounding input for generation.
    /// </summary>
    public required Guid SourceId { get; set; }

    /// <summary>
    ///     Source used as the grounding input for generation.
    /// </summary>
    public Source Source { get; set; } = null!;

    /// <summary>
    ///     Generated space when execution completes.
    /// </summary>
    public Guid? CreatedSpaceId { get; set; }

    /// <summary>
    ///     Generated space when execution completes.
    /// </summary>
    public Space? CreatedSpace { get; set; }

    /// <summary>
    ///     Current lifecycle state of the generation run.
    /// </summary>
    public required SourceGenerationRunStatus Status { get; set; }

    /// <summary>
    ///     User-correctable or execution failure description.
    /// </summary>
    public string? FailureReason { get; set; }

    /// <summary>
    ///     Non-blocking validation or grounding warning.
    /// </summary>
    public string? Warning { get; set; }

    /// <summary>
    ///     Requested generated space name.
    /// </summary>
    public required string SpaceName { get; set; }

    /// <summary>
    ///     Requested generated space slug.
    /// </summary>
    public string? SpaceSlug { get; set; }

    /// <summary>
    ///     Requested generated space language.
    /// </summary>
    public required string Language { get; set; }

    /// <summary>
    ///     Review-safe visibility requested for generated QnA content.
    /// </summary>
    public required VisibilityScope Visibility { get; set; }

    /// <summary>
    ///     Review-safe lifecycle state requested for the generated space.
    /// </summary>
    public required SpaceStatus SpaceStatus { get; set; }

    /// <summary>
    ///     Indicates whether the generated space accepts new questions.
    /// </summary>
    public required bool AcceptsQuestions { get; set; }

    /// <summary>
    ///     Indicates whether the generated space accepts answers.
    /// </summary>
    public required bool AcceptsAnswers { get; set; }

    /// <summary>
    ///     Optional goal or audience note used while generating draft content.
    /// </summary>
    public string? ExtractionGoal { get; set; }

    /// <summary>
    ///     Maximum top-level questions requested for generation.
    /// </summary>
    public required int MaxTopLevelQuestions { get; set; }

    /// <summary>
    ///     Maximum follow-up depth requested for generation.
    /// </summary>
    public required int MaxFollowUpDepth { get; set; }

    /// <summary>
    ///     Maximum answers requested per generated question.
    /// </summary>
    public required int MaxAnswersPerQuestion { get; set; }

    /// <summary>
    ///     Indicates whether follow-up questions should be generated.
    /// </summary>
    public required bool IncludeFollowUpQuestions { get; set; }

    /// <summary>
    ///     Requested tag write behavior.
    /// </summary>
    public required SourceGenerationTagMode TagGenerationMode { get; set; }

    /// <summary>
    ///     Source role applied to generated relationships.
    /// </summary>
    public required SourceRole SourceRole { get; set; }

    /// <summary>
    ///     Indicates whether each generated answer must cite the source.
    /// </summary>
    public required bool RequireEveryAnswerToCiteSource { get; set; }

    /// <summary>
    ///     Optional content range or section hint for long sources.
    /// </summary>
    public string? ContentHint { get; set; }

    /// <summary>
    ///     Optional raw structured generator output for audit/debug.
    /// </summary>
    public string? RawOutputJson { get; set; }

    /// <summary>
    ///     UTC timestamp when execution started.
    /// </summary>
    public DateTime? StartedAtUtc { get; set; }

    /// <summary>
    ///     UTC timestamp when execution completed or failed.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    ///     Tenant that owns the run.
    /// </summary>
    public required Guid TenantId { get; set; }
}
