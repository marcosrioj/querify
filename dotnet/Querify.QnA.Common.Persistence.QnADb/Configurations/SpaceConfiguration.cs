using Querify.Common.EntityFramework.Core.Configurations;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Querify.QnA.Common.Persistence.QnADb.Configurations;

public class SpaceConfiguration : BaseConfiguration<Space>
{
    public override void Configure(EntityTypeBuilder<Space> builder)
    {
        base.Configure(builder);

        builder.ToTable("Spaces");

        builder.Property(space => space.Name)
            .HasMaxLength(Space.MaxNameLength)
            .IsRequired();

        builder.Property(space => space.Slug)
            .HasMaxLength(Space.MaxSlugLength)
            .IsRequired();

        builder.Property(space => space.Summary)
            .HasMaxLength(Space.MaxSummaryLength);

        builder.Property(space => space.Language)
            .HasMaxLength(Space.MaxLanguageLength)
            .IsRequired();

        builder.Property(space => space.Visibility)
            .HasDefaultValue(VisibilityScope.Internal)
            .IsRequired();

        builder.Property(space => space.TenantId)
            .IsRequired();

        builder.HasIndex(space => new { space.TenantId, space.Slug })
            .HasDatabaseName("IX_Space_TenantId_Slug")
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
