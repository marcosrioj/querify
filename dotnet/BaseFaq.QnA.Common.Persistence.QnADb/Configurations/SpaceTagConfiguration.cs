using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class SpaceTagConfiguration : BaseConfiguration<SpaceTag>
{
    public override void Configure(EntityTypeBuilder<SpaceTag> builder)
    {
        base.Configure(builder);

        builder.ToTable("SpaceTags");

        builder.Property(link => link.TenantId)
            .IsRequired();

        builder.Property(link => link.SpaceId)
            .IsRequired();

        builder.Property(link => link.TagId)
            .IsRequired();

        builder.HasIndex(link => new { link.SpaceId, link.TagId })
            .HasDatabaseName("IX_SpaceTag_SpaceId_TagId")
            .IsUnique();
    }
}