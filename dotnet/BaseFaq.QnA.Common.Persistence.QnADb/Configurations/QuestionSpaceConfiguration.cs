using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class QuestionSpaceConfiguration : BaseConfiguration<QuestionSpace>
{
    public override void Configure(EntityTypeBuilder<QuestionSpace> builder)
    {
        base.Configure(builder);

        builder.ToTable("QuestionSpaces");

        builder.Property(space => space.Name)
            .HasMaxLength(QuestionSpace.MaxNameLength)
            .IsRequired();

        builder.Property(space => space.Key)
            .HasMaxLength(QuestionSpace.MaxKeyLength)
            .IsRequired();

        builder.Property(space => space.Summary)
            .HasMaxLength(QuestionSpace.MaxSummaryLength);

        builder.Property(space => space.DefaultLanguage)
            .HasMaxLength(QuestionSpace.MaxLanguageLength)
            .IsRequired();

        builder.Property(space => space.ProductScope)
            .HasMaxLength(QuestionSpace.MaxProductScopeLength);

        builder.Property(space => space.JourneyScope)
            .HasMaxLength(QuestionSpace.MaxJourneyScopeLength);

        builder.Property(space => space.TenantId)
            .IsRequired();

        builder.HasIndex(space => new { space.TenantId, space.Key })
            .HasDatabaseName("IX_QuestionSpace_TenantId_Key")
            .IsUnique();

        builder.HasMany(space => space.Questions)
            .WithOne(question => question.Space)
            .HasForeignKey(question => question.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(space => space.Topics)
            .WithOne(link => link.QuestionSpace)
            .HasForeignKey(link => link.QuestionSpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(space => space.Sources)
            .WithOne(link => link.QuestionSpace)
            .HasForeignKey(link => link.QuestionSpaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
