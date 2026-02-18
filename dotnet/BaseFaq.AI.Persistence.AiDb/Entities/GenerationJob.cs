using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.Ai.Enums;

namespace BaseFaq.AI.Persistence.AiDb.Entities;

public class GenerationJob : BaseEntity
{
    public const int MaxLanguageLength = 16;
    public const int MaxPromptProfileLength = 128;
    public const int MaxIdempotencyKeyLength = 128;
    public const int MaxProviderLength = 100;
    public const int MaxModelLength = 100;
    public const int MaxErrorCodeLength = 100;
    public const int MaxErrorMessageLength = 4000;

    public required Guid CorrelationId { get; set; }
    public required Guid RequestedByUserId { get; set; }
    public required Guid FaqId { get; set; }

    public required string Language { get; set; }
    public required string PromptProfile { get; set; }
    public required string IdempotencyKey { get; set; }

    public GenerationJobStatus Status { get; set; } = GenerationJobStatus.Requested;

    public DateTime RequestedUtc { get; set; }
    public DateTime? StartedUtc { get; set; }
    public DateTime? CompletedUtc { get; set; }

    public string? Provider { get; set; }
    public string? Model { get; set; }

    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public ICollection<GenerationArtifact> Artifacts { get; set; } = [];
}