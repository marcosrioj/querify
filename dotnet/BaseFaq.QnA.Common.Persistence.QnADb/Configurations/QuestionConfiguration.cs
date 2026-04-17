using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
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

        builder.Property(question => question.Key)
            .HasMaxLength(Question.MaxKeyLength)
            .IsRequired();

        builder.Property(question => question.Summary)
            .HasMaxLength(Question.MaxSummaryLength);

        builder.Property(question => question.ContextNote)
            .HasMaxLength(Question.MaxContextNoteLength);

        builder.Property(question => question.Language)
            .HasMaxLength(Question.MaxLanguageLength);

        builder.Property(question => question.ProductScope)
            .HasMaxLength(Question.MaxProductScopeLength);

        builder.Property(question => question.JourneyScope)
            .HasMaxLength(Question.MaxJourneyScopeLength);

        builder.Property(question => question.AudienceScope)
            .HasMaxLength(Question.MaxAudienceScopeLength);

        builder.Property(question => question.ContextKey)
            .HasMaxLength(Question.MaxContextKeyLength);

        builder.Property(question => question.OriginUrl)
            .HasMaxLength(Question.MaxOriginUrlLength);

        builder.Property(question => question.OriginReference)
            .HasMaxLength(Question.MaxOriginReferenceLength);

        builder.Property(question => question.ThreadSummary)
            .HasMaxLength(Question.MaxThreadSummaryLength);

        builder.Property(question => question.TenantId)
            .IsRequired();

        builder.Property(question => question.SpaceId)
            .IsRequired();

        builder.HasIndex(question => new { question.TenantId, question.Key })
            .HasDatabaseName("IX_Question_TenantId_Key")
            .IsUnique();

        builder.HasOne(question => question.AcceptedAnswer)
            .WithMany()
            .HasForeignKey(question => question.AcceptedAnswerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(question => question.DuplicateOfQuestion)
            .WithMany(question => question.DuplicateQuestions)
            .HasForeignKey(question => question.DuplicateOfQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

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