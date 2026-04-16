using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Faq.Common.Persistence.FaqDb.Configurations;

public class FaqItemConfiguration : BaseConfiguration<FaqItem>
{
    public override void Configure(EntityTypeBuilder<FaqItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("FaqItems");

        builder.Property(p => p.Question)
            .HasMaxLength(FaqItem.MaxQuestionLength)
            .IsRequired();

        builder.Property(p => p.AdditionalInfo)
            .HasMaxLength(FaqItem.MaxAdditionalInfoLength);

        builder.Property(p => p.CtaTitle)
            .HasMaxLength(FaqItem.MaxCtaTitleLength);

        builder.Property(p => p.CtaUrl)
            .HasMaxLength(FaqItem.MaxCtaUrlLength);

        builder.Property(p => p.Sort)
            .IsRequired();

        builder.Property(p => p.FeedbackScore)
            .IsRequired();

        builder.Property(p => p.ConfidenceScore)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.FaqId)
            .IsRequired();

        builder.HasIndex(p => p.FaqId)
            .HasDatabaseName("IX_FaqItem_FaqId");

        builder.HasMany(p => p.Answers)
            .WithOne(p => p.FaqItem)
            .HasForeignKey(p => p.FaqItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Feedbacks)
            .WithOne(p => p.FaqItem)
            .HasForeignKey(p => p.FaqItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
