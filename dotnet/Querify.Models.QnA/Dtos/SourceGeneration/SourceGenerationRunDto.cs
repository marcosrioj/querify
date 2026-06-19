using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.SourceGeneration;

public sealed class SourceGenerationRunDto
{
    public required Guid Id { get; set; }
    public required Guid SourceId { get; set; }
    public Guid? CreatedSpaceId { get; set; }
    public required SourceGenerationRunStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public string? Warning { get; set; }
    public required string SpaceName { get; set; }
    public string? SpaceSlug { get; set; }
    public required string Language { get; set; }
    public required VisibilityScope Visibility { get; set; }
    public required SpaceStatus SpaceStatus { get; set; }
    public required bool AcceptsQuestions { get; set; }
    public required bool AcceptsAnswers { get; set; }
    public string? ExtractionGoal { get; set; }
    public required int MaxTopLevelQuestions { get; set; }
    public required int MaxFollowUpDepth { get; set; }
    public required int MaxAnswersPerQuestion { get; set; }
    public required bool IncludeFollowUpQuestions { get; set; }
    public required SourceGenerationTagMode TagGenerationMode { get; set; }
    public required SourceRole SourceRole { get; set; }
    public required bool RequireEveryAnswerToCiteSource { get; set; }
    public string? ContentHint { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
