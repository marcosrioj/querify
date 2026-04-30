using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class SpaceSourceConfiguration : BaseConfiguration<SpaceSource>
{
    public override void Configure(EntityTypeBuilder<SpaceSource> builder)
    {
        base.Configure(builder);

        builder.ToTable("SpaceSources");

        builder.Property(link => link.TenantId)
            .IsRequired();

        builder.Property(link => link.SpaceId)
            .IsRequired();

        builder.Property(link => link.SourceId)
            .IsRequired();

        builder.HasIndex(link => new { link.SpaceId, link.SourceId })
            .HasDatabaseName("IX_SpaceSource_SpaceId_SourceId")
            .IsUnique();
    }
}