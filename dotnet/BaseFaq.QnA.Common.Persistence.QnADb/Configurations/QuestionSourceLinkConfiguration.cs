using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class QuestionSourceLinkConfiguration : BaseConfiguration<QuestionSourceLink>
{
    public override void Configure(EntityTypeBuilder<QuestionSourceLink> builder)
    {
        base.Configure(builder);

        builder.ToTable("QuestionSourceLinks");

        builder.Property(link => link.TenantId)
            .IsRequired();

        builder.Property(link => link.QuestionId)
            .IsRequired();

        builder.Property(link => link.SourceId)
            .IsRequired();

        builder.Property(link => link.Label)
            .HasMaxLength(QuestionSourceLink.MaxLabelLength);

        builder.Property(link => link.Scope)
            .HasMaxLength(QuestionSourceLink.MaxScopeLength);

        builder.Property(link => link.Excerpt)
            .HasMaxLength(QuestionSourceLink.MaxExcerptLength);

        builder.HasIndex(link => new { link.QuestionId, link.SourceId, link.Role, link.Order })
            .HasDatabaseName("IX_QuestionSourceLink_QuestionId_SourceId_Role_Order")
            .IsUnique();
    }
}
