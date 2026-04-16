using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class QuestionSpaceTopicConfiguration : BaseConfiguration<QuestionSpaceTopic>
{
    public override void Configure(EntityTypeBuilder<QuestionSpaceTopic> builder)
    {
        base.Configure(builder);

        builder.ToTable("QuestionSpaceTopics");

        builder.Property(link => link.TenantId)
            .IsRequired();

        builder.Property(link => link.QuestionSpaceId)
            .IsRequired();

        builder.Property(link => link.TopicId)
            .IsRequired();

        builder.HasIndex(link => new { link.QuestionSpaceId, link.TopicId })
            .HasDatabaseName("IX_QuestionSpaceTopic_QuestionSpaceId_TopicId")
            .IsUnique();
    }
}
