using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class QuestionTopicConfiguration : BaseConfiguration<QuestionTopic>
{
    public override void Configure(EntityTypeBuilder<QuestionTopic> builder)
    {
        base.Configure(builder);

        builder.ToTable("QuestionTopics");

        builder.Property(link => link.TenantId)
            .IsRequired();

        builder.Property(link => link.QuestionId)
            .IsRequired();

        builder.Property(link => link.TopicId)
            .IsRequired();

        builder.HasIndex(link => new { link.QuestionId, link.TopicId })
            .HasDatabaseName("IX_QuestionTopic_QuestionId_TopicId")
            .IsUnique();
    }
}
