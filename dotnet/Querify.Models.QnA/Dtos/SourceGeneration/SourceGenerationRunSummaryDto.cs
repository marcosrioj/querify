using Querify.Models.QnA.Enums;

namespace Querify.Models.QnA.Dtos.SourceGeneration;

public sealed class SourceGenerationRunSummaryDto
{
    public required Guid Id { get; set; }
    public required Guid SourceId { get; set; }
    public Guid? CreatedSpaceId { get; set; }
    public required SourceGenerationRunStatus Status { get; set; }
    public string? FailureReason { get; set; }
    public required string SpaceName { get; set; }
    public required SourceGenerationTagMode TagGenerationMode { get; set; }
    public required DateTime CreatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
