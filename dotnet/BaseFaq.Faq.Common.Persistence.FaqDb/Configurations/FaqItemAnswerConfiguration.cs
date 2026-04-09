using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Faq.Common.Persistence.FaqDb.Configurations;

public class FaqItemAnswerConfiguration : BaseConfiguration<FaqItemAnswer>
{
    public override void Configure(EntityTypeBuilder<FaqItemAnswer> builder)
    {
        base.Configure(builder);

        builder.ToTable("FaqItemAnswers");

        builder.Property(p => p.ShortAnswer)
            .HasMaxLength(FaqItemAnswer.MaxShortAnswerLength)
            .IsRequired();

        builder.Property(p => p.Answer)
            .HasMaxLength(FaqItemAnswer.MaxAnswerLength);

        builder.Property(p => p.Sort)
            .IsRequired();

        builder.Property(p => p.VoteScore)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.FaqItemId)
            .IsRequired();

        builder.HasIndex(p => p.FaqItemId)
            .HasDatabaseName("IX_FaqItemAnswer_FaqItemId");

        builder.HasMany(p => p.Votes)
            .WithOne(p => p.FaqItemAnswer)
            .HasForeignKey(p => p.FaqItemAnswerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
