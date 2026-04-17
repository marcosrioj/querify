using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class SpaceConfiguration : BaseConfiguration<Space>
{
    public override void Configure(EntityTypeBuilder<Space> builder)
    {
        base.Configure(builder);

        builder.ToTable("Spaces");

        builder.Property(space => space.Name)
            .HasMaxLength(Space.MaxNameLength)
            .IsRequired();

        builder.Property(space => space.Key)
            .HasMaxLength(Space.MaxKeyLength)
            .IsRequired();

        builder.Property(space => space.Summary)
            .HasMaxLength(Space.MaxSummaryLength);

        builder.Property(space => space.DefaultLanguage)
            .HasMaxLength(Space.MaxLanguageLength)
            .IsRequired();

        builder.Property(space => space.ProductScope)
            .HasMaxLength(Space.MaxProductScopeLength);

        builder.Property(space => space.JourneyScope)
            .HasMaxLength(Space.MaxJourneyScopeLength);

        builder.Property(space => space.TenantId)
            .IsRequired();

        builder.HasIndex(space => new { space.TenantId, space.Key })
            .HasDatabaseName("IX_Space_TenantId_Key")
            .IsUnique();

        builder.HasMany(space => space.Questions)
            .WithOne(question => question.Space)
            .HasForeignKey(question => question.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(space => space.Tags)
            .WithOne(link => link.Space)
            .HasForeignKey(link => link.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(space => space.Sources)
            .WithOne(link => link.Space)
            .HasForeignKey(link => link.SpaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}