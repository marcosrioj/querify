using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class QuestionConfiguration : BaseConfiguration<Question>
{
    public override void Configure(EntityTypeBuilder<Question> builder)
    {
        base.Configure(builder);

        builder.ToTable("Questions");

        builder.Property(question => question.Title)
            .HasMaxLength(Question.MaxTitleLength)
            .IsRequired();

        builder.Property(question => question.Summary)
            .HasMaxLength(Question.MaxSummaryLength);

        builder.Property(question => question.ContextNote)
            .HasMaxLength(Question.MaxContextNoteLength);

        builder.Property(question => question.Visibility)
            .HasDefaultValue(VisibilityScope.Internal)
            .IsRequired();

        builder.Property(question => question.AiConfidenceScore)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(question => question.TenantId)
            .IsRequired();

        builder.Property(question => question.SpaceId)
            .IsRequired();

        builder.HasOne(question => question.AcceptedAnswer)
            .WithMany()
            .HasForeignKey(question => question.AcceptedAnswerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(question => question.Answers)
            .WithOne(answer => answer.Question)
            .HasForeignKey(answer => answer.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(question => question.Sources)
            .WithOne(sourceLink => sourceLink.Question)
            .HasForeignKey(sourceLink => sourceLink.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(question => question.Tags)
            .WithOne(tagLink => tagLink.Question)
            .HasForeignKey(tagLink => tagLink.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(question => question.Activities)
            .WithOne(activity => activity.Question)
            .HasForeignKey(activity => activity.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
