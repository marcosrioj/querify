using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class TagConfiguration : BaseConfiguration<Tag>
{
    public override void Configure(EntityTypeBuilder<Tag> builder)
    {
        base.Configure(builder);

        builder.ToTable("Tags");

        builder.Property(tag => tag.Name)
            .HasMaxLength(Tag.MaxNameLength)
            .IsRequired();

        builder.Property(tag => tag.TenantId)
            .IsRequired();

        builder.HasIndex(tag => new { tag.TenantId, tag.Name })
            .HasDatabaseName("IX_Tag_TenantId_Name");

        builder.HasMany(tag => tag.Spaces)
            .WithOne(link => link.Tag)
            .HasForeignKey(link => link.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(tag => tag.Questions)
            .WithOne(link => link.Tag)
            .HasForeignKey(link => link.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
