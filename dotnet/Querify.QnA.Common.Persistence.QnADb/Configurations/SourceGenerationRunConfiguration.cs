using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Querify.Common.EntityFramework.Core.Configurations;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.Entities;

namespace Querify.QnA.Common.Persistence.QnADb.Configurations;

public class SourceGenerationRunConfiguration : BaseConfiguration<SourceGenerationRun>
{
    public override void Configure(EntityTypeBuilder<SourceGenerationRun> builder)
    {
        base.Configure(builder);

        builder.ToTable("SourceGenerationRuns");

        builder.Property(run => run.TenantId)
            .IsRequired();

        builder.Property(run => run.SourceId)
            .IsRequired();

        builder.Property(run => run.Status)
            .HasConversion<int>()
            .HasDefaultValue(SourceGenerationRunStatus.Pending)
            .IsRequired();

        builder.Property(run => run.SpaceName)
            .HasMaxLength(SourceGenerationRun.MaxSpaceNameLength)
            .IsRequired();

        builder.Property(run => run.SpaceSlug)
            .HasMaxLength(SourceGenerationRun.MaxSpaceSlugLength);

        builder.Property(run => run.Language)
            .HasMaxLength(SourceGenerationRun.MaxLanguageLength)
            .IsRequired();

        builder.Property(run => run.Visibility)
            .HasConversion<int>()
            .HasDefaultValue(VisibilityScope.Internal)
            .IsRequired();

        builder.Property(run => run.SpaceStatus)
            .HasConversion<int>()
            .HasDefaultValue(SpaceStatus.Draft)
            .IsRequired();

        builder.Property(run => run.ExtractionGoal)
            .HasMaxLength(SourceGenerationRun.MaxExtractionGoalLength);

        builder.Property(run => run.TagGenerationMode)
            .HasConversion<int>()
            .HasDefaultValue(SourceGenerationTagMode.CreateAndAttach)
            .IsRequired();

        builder.Property(run => run.SourceRole)
            .HasConversion<int>()
            .HasDefaultValue(SourceRole.Origin)
            .IsRequired();

        builder.Property(run => run.ContentHint)
            .HasMaxLength(SourceGenerationRun.MaxContentHintLength);

        builder.Property(run => run.FailureReason)
            .HasMaxLength(SourceGenerationRun.MaxFailureReasonLength);

        builder.Property(run => run.Warning)
            .HasMaxLength(SourceGenerationRun.MaxWarningLength);

        builder.Property(run => run.RawOutputJson)
            .HasMaxLength(SourceGenerationRun.MaxRawOutputJsonLength);

        builder.HasOne(run => run.Source)
            .WithMany()
            .HasForeignKey(run => run.SourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(run => run.CreatedSpace)
            .WithMany()
            .HasForeignKey(run => run.CreatedSpaceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(run => new { run.TenantId, run.SourceId, run.CreatedDate })
            .HasDatabaseName("IX_SourceGenerationRuns_TenantId_SourceId_CreatedDate");

        builder.HasIndex(run => new { run.TenantId, run.Status })
            .HasDatabaseName("IX_SourceGenerationRuns_TenantId_Status");
    }
}
