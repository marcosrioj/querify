using BaseFaq.AI.Persistence.AiDb.Entities;
using BaseFaq.Common.EntityFramework.Core.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.AI.Persistence.AiDb.Configurations;

public class GenerationJobConfiguration : BaseConfiguration<GenerationJob>
{
    public override void Configure(EntityTypeBuilder<GenerationJob> builder)
    {
        base.Configure(builder);

        builder.ToTable("GenerationJobs");

        builder.Property(p => p.CorrelationId)
            .IsRequired();

        builder.Property(p => p.RequestedByUserId)
            .IsRequired();

        builder.Property(p => p.FaqId)
            .IsRequired();

        builder.Property(p => p.Language)
            .HasMaxLength(GenerationJob.MaxLanguageLength)
            .IsRequired();

        builder.Property(p => p.PromptProfile)
            .HasMaxLength(GenerationJob.MaxPromptProfileLength)
            .IsRequired();

        builder.Property(p => p.IdempotencyKey)
            .HasMaxLength(GenerationJob.MaxIdempotencyKeyLength)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.RequestedUtc)
            .IsRequired();

        builder.Property(p => p.Provider)
            .HasMaxLength(GenerationJob.MaxProviderLength)
            .IsRequired(false);

        builder.Property(p => p.Model)
            .HasMaxLength(GenerationJob.MaxModelLength)
            .IsRequired(false);

        builder.Property(p => p.ErrorCode)
            .HasMaxLength(GenerationJob.MaxErrorCodeLength)
            .IsRequired(false);

        builder.Property(p => p.ErrorMessage)
            .HasMaxLength(GenerationJob.MaxErrorMessageLength)
            .IsRequired(false);

        builder.HasIndex(p => p.CorrelationId)
            .HasDatabaseName("IX_GenerationJob_CorrelationId")
            .IsUnique();

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_GenerationJob_Status");

        builder.HasIndex(p => p.FaqId)
            .HasDatabaseName("IX_GenerationJob_FaqId");

        builder.HasIndex(p => p.IdempotencyKey)
            .HasDatabaseName("IX_GenerationJob_IdempotencyKey");

        builder.HasIndex(p => new { p.FaqId, p.IdempotencyKey })
            .HasDatabaseName("IX_GenerationJob_FaqId_IdempotencyKey")
            .IsUnique();

        builder.HasMany(p => p.Artifacts)
            .WithOne(item => item.GenerationJob)
            .HasForeignKey(item => item.GenerationJobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}