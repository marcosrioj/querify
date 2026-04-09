using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Faq.Common.Persistence.FaqDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Faq.Common.Persistence.FaqDb.Configurations;

public class VoteConfiguration : BaseConfiguration<Vote>
{
    public override void Configure(EntityTypeBuilder<Vote> builder)
    {
        base.Configure(builder);

        builder.ToTable("Votes");

        builder.Property(p => p.UserPrint)
            .HasMaxLength(Vote.MaxUserPrintLength)
            .IsRequired();

        builder.Property(p => p.Ip)
            .HasMaxLength(Vote.MaxIpLength)
            .IsRequired();

        builder.Property(p => p.UserAgent)
            .HasMaxLength(Vote.MaxUserAgentLength)
            .IsRequired();

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.FaqItemAnswerId)
            .IsRequired();

        builder.HasIndex(p => p.FaqItemAnswerId)
            .HasDatabaseName("IX_Vote_FaqItemAnswerId");
    }
}
