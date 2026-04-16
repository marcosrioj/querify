using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class QuestionSpaceSourceConfiguration : BaseConfiguration<QuestionSpaceSource>
{
    public override void Configure(EntityTypeBuilder<QuestionSpaceSource> builder)
    {
        base.Configure(builder);

        builder.ToTable("QuestionSpaceSources");

        builder.Property(link => link.TenantId)
            .IsRequired();

        builder.Property(link => link.QuestionSpaceId)
            .IsRequired();

        builder.Property(link => link.KnowledgeSourceId)
            .IsRequired();

        builder.HasIndex(link => new { link.QuestionSpaceId, link.KnowledgeSourceId })
            .HasDatabaseName("IX_QuestionSpaceSource_QuestionSpaceId_KnowledgeSourceId")
            .IsUnique();
    }
}