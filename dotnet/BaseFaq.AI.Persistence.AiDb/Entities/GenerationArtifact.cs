using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.Ai.Enums;

namespace BaseFaq.AI.Persistence.AiDb.Entities;

public class GenerationArtifact : BaseEntity
{
    public const int MaxContentLength = 16000;
    public const int MaxMetadataJsonLength = 8000;

    public required Guid GenerationJobId { get; set; }
    public GenerationJob GenerationJob { get; set; } = null!;

    public GenerationArtifactType ArtifactType { get; set; }
    public int Sequence { get; set; }

    public required string Content { get; set; }
    public string? MetadataJson { get; set; }
}