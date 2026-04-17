using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class QuestionTagConfiguration : BaseConfiguration<QuestionTag>
{
    public override void Configure(EntityTypeBuilder<QuestionTag> builder)
    {
        base.Configure(builder);

        builder.ToTable("QuestionTags");

        builder.Property(link => link.TenantId)
            .IsRequired();

        builder.Property(link => link.QuestionId)
            .IsRequired();

        builder.Property(link => link.TagId)
            .IsRequired();

        builder.HasIndex(link => new { link.QuestionId, link.TagId })
            .HasDatabaseName("IX_QuestionTag_QuestionId_TagId")
            .IsUnique();
    }
}