using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Faq.Common.Persistence.FaqDb.Configurations;

public class FeedbackConfiguration : BaseConfiguration<Feedback>
{
    public override void Configure(EntityTypeBuilder<Feedback> builder)
    {
        base.Configure(builder);

        builder.ToTable("Feedbacks");

        builder.Property(p => p.Like)
            .IsRequired();

        builder.Property(p => p.UserPrint)
            .HasMaxLength(Feedback.MaxUserPrintLength)
            .IsRequired();

        builder.Property(p => p.Ip)
            .HasMaxLength(Feedback.MaxIpLength)
            .IsRequired();

        builder.Property(p => p.UserAgent)
            .HasMaxLength(Feedback.MaxUserAgentLength)
            .IsRequired();

        builder.Property(p => p.UnLikeReason);

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.FaqItemId)
            .IsRequired();

        builder.HasIndex(p => p.FaqItemId)
            .HasDatabaseName("IX_Feedback_FaqItemId");
    }
}