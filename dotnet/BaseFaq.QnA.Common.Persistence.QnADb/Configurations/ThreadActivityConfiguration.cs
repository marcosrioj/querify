using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class ThreadActivityConfiguration : BaseConfiguration<ThreadActivity>
{
    public override void Configure(EntityTypeBuilder<ThreadActivity> builder)
    {
        base.Configure(builder);

        builder.ToTable("ThreadActivities");

        builder.Property(activity => activity.TenantId)
            .IsRequired();

        builder.Property(activity => activity.QuestionId)
            .IsRequired();

        builder.Property(activity => activity.ActorLabel)
            .HasMaxLength(ThreadActivity.MaxActorLabelLength);

        builder.Property(activity => activity.Notes)
            .HasMaxLength(ThreadActivity.MaxNotesLength);

        builder.Property(activity => activity.MetadataJson)
            .HasMaxLength(ThreadActivity.MaxMetadataLength);

        builder.Property(activity => activity.OccurredAtUtc)
            .IsRequired();

        builder.HasIndex(activity => new { activity.QuestionId, activity.OccurredAtUtc })
            .HasDatabaseName("IX_ThreadActivity_QuestionId_OccurredAtUtc");
    }
}
