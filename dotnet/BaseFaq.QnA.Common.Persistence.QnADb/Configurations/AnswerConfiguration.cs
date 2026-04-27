using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class AnswerConfiguration : BaseConfiguration<Answer>
{
    public override void Configure(EntityTypeBuilder<Answer> builder)
    {
        base.Configure(builder);

        builder.ToTable("Answers");

        builder.Property(answer => answer.Headline)
            .HasMaxLength(Answer.MaxHeadlineLength)
            .IsRequired();

        builder.Property(answer => answer.Body)
            .HasMaxLength(Answer.MaxBodyLength);

        builder.Property(answer => answer.ContextNote)
            .HasMaxLength(Answer.MaxContextNoteLength);

        builder.Property(answer => answer.AuthorLabel)
            .HasMaxLength(Answer.MaxAuthorLabelLength);

        builder.Property(answer => answer.AiConfidenceScore)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(answer => answer.TenantId)
            .IsRequired();

        builder.Property(answer => answer.QuestionId)
            .IsRequired();

        builder.HasMany(answer => answer.Sources)
            .WithOne(sourceLink => sourceLink.Answer)
            .HasForeignKey(sourceLink => sourceLink.AnswerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
