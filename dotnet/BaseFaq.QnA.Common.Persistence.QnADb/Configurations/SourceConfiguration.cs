using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class SourceConfiguration : BaseConfiguration<Source>
{
    public override void Configure(EntityTypeBuilder<Source> builder)
    {
        base.Configure(builder);

        builder.ToTable("Sources");

        builder.Property(source => source.Locator)
            .HasMaxLength(Source.MaxLocatorLength)
            .IsRequired();

        builder.Property(source => source.Label)
            .HasMaxLength(Source.MaxLabelLength);

        builder.Property(source => source.Scope)
            .HasMaxLength(Source.MaxScopeLength);

        builder.Property(source => source.SystemName)
            .HasMaxLength(Source.MaxSystemNameLength);

        builder.Property(source => source.ExternalId)
            .HasMaxLength(Source.MaxExternalIdLength);

        builder.Property(source => source.Language)
            .HasMaxLength(Source.MaxLanguageLength);

        builder.Property(source => source.MediaType)
            .HasMaxLength(Source.MaxMediaTypeLength);

        builder.Property(source => source.Checksum)
            .HasMaxLength(Source.MaxChecksumLength);

        builder.Property(source => source.MetadataJson)
            .HasMaxLength(Source.MaxMetadataLength);

        builder.Property(source => source.TenantId)
            .IsRequired();

        builder.HasMany(source => source.Spaces)
            .WithOne(link => link.Source)
            .HasForeignKey(link => link.SourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(source => source.Questions)
            .WithOne(link => link.Source)
            .HasForeignKey(link => link.SourceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(source => source.Answers)
            .WithOne(link => link.Source)
            .HasForeignKey(link => link.SourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}