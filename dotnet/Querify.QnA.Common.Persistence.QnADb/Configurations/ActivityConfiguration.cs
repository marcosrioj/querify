using Querify.Common.EntityFramework.Core.Configurations;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Querify.QnA.Common.Persistence.QnADb.Configurations;

public class ActivityConfiguration : BaseConfiguration<Activity>
{
    public override void Configure(EntityTypeBuilder<Activity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Activities");

        builder.Property(activity => activity.TenantId)
            .IsRequired();

        builder.Property(activity => activity.QuestionId)
            .IsRequired();

        builder.Property(activity => activity.ActorLabel)
            .HasMaxLength(Activity.MaxActorLabelLength);

        builder.Property(activity => activity.UserPrint)
            .HasMaxLength(Activity.MaxUserPrintLength)
            .IsRequired();

        builder.Property(activity => activity.Ip)
            .HasMaxLength(Activity.MaxIpLength)
            .IsRequired();

        builder.Property(activity => activity.UserAgent)
            .HasMaxLength(Activity.MaxUserAgentLength)
            .IsRequired();

        builder.Property(activity => activity.Notes)
            .HasMaxLength(Activity.MaxNotesLength);

        builder.Property(activity => activity.MetadataJson)
            .HasMaxLength(Activity.MaxMetadataLength);

        builder.Property(activity => activity.OccurredAtUtc)
            .IsRequired();

        builder.HasIndex(activity => new { activity.QuestionId, activity.OccurredAtUtc })
            .HasDatabaseName("IX_Activity_QuestionId_OccurredAtUtc");
    }
}