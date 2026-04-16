using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class AnswerSourceLinkConfiguration : BaseConfiguration<AnswerSourceLink>
{
    public override void Configure(EntityTypeBuilder<AnswerSourceLink> builder)
    {
        base.Configure(builder);

        builder.ToTable("AnswerSourceLinks");

        builder.Property(link => link.TenantId)
            .IsRequired();

        builder.Property(link => link.AnswerId)
            .IsRequired();

        builder.Property(link => link.SourceId)
            .IsRequired();

        builder.Property(link => link.Label)
            .HasMaxLength(AnswerSourceLink.MaxLabelLength);

        builder.Property(link => link.Scope)
            .HasMaxLength(AnswerSourceLink.MaxScopeLength);

        builder.Property(link => link.Excerpt)
            .HasMaxLength(AnswerSourceLink.MaxExcerptLength);

        builder.HasIndex(link => new { link.AnswerId, link.SourceId, link.Role, link.Order })
            .HasDatabaseName("IX_AnswerSourceLink_AnswerId_SourceId_Role_Order")
            .IsUnique();
    }
}
