using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class TopicConfiguration : BaseConfiguration<Topic>
{
    public override void Configure(EntityTypeBuilder<Topic> builder)
    {
        base.Configure(builder);

        builder.ToTable("Topics");

        builder.Property(topic => topic.Name)
            .HasMaxLength(Topic.MaxNameLength)
            .IsRequired();

        builder.Property(topic => topic.Category)
            .HasMaxLength(Topic.MaxCategoryLength);

        builder.Property(topic => topic.Description)
            .HasMaxLength(Topic.MaxDescriptionLength);

        builder.Property(topic => topic.TenantId)
            .IsRequired();

        builder.HasIndex(topic => new { topic.TenantId, topic.Name })
            .HasDatabaseName("IX_Topic_TenantId_Name");

        builder.HasMany(topic => topic.QuestionSpaceTopics)
            .WithOne(link => link.Topic)
            .HasForeignKey(link => link.TopicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(topic => topic.QuestionTopics)
            .WithOne(link => link.Topic)
            .HasForeignKey(link => link.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}