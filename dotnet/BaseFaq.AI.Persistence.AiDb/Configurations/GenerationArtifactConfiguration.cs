using BaseFaq.AI.Persistence.AiDb.Entities;
using BaseFaq.Common.EntityFramework.Core.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.AI.Persistence.AiDb.Configurations;

public class GenerationArtifactConfiguration : BaseConfiguration<GenerationArtifact>
{
    public override void Configure(EntityTypeBuilder<GenerationArtifact> builder)
    {
        base.Configure(builder);

        builder.ToTable("GenerationArtifacts");

        builder.Property(p => p.GenerationJobId)
            .IsRequired();

        builder.Property(p => p.ArtifactType)
            .IsRequired();

        builder.Property(p => p.Sequence)
            .IsRequired();

        builder.Property(p => p.Content)
            .HasMaxLength(GenerationArtifact.MaxContentLength)
            .IsRequired();

        builder.Property(p => p.MetadataJson)
            .HasMaxLength(GenerationArtifact.MaxMetadataJsonLength)
            .IsRequired(false);

        builder.HasIndex(p => p.GenerationJobId)
            .HasDatabaseName("IX_GenerationArtifact_GenerationJobId");

        builder.HasIndex(p => new { p.GenerationJobId, p.Sequence })
            .HasDatabaseName("IX_GenerationArtifact_GenerationJobId_Sequence")
            .IsUnique();
    }
}