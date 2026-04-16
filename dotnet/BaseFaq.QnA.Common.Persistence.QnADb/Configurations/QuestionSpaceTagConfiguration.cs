using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class QuestionSpaceTagConfiguration : BaseConfiguration<QuestionSpaceTag>
{
    public override void Configure(EntityTypeBuilder<QuestionSpaceTag> builder)
    {
        base.Configure(builder);

        builder.ToTable("QuestionSpaceTags");

        builder.Property(link => link.TenantId)
            .IsRequired();

        builder.Property(link => link.QuestionSpaceId)
            .IsRequired();

        builder.Property(link => link.TagId)
            .IsRequired();

        builder.HasIndex(link => new { link.QuestionSpaceId, link.TagId })
            .HasDatabaseName("IX_QuestionSpaceTag_QuestionSpaceId_TagId")
            .IsUnique();
    }
}