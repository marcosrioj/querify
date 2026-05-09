using Querify.Common.EntityFramework.Core.Configurations;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Querify.QnA.Common.Persistence.QnADb.Configurations;

public class SourceConfiguration : BaseConfiguration<Source>
{
    public override void Configure(EntityTypeBuilder<Source> builder)
    {
        base.Configure(builder);

        builder.ToTable("Sources");

        builder.Property(source => source.Locator)
            .HasMaxLength(Source.MaxLocatorLength)
            .IsRequired();

        builder.Property(source => source.StorageKey)
            .HasMaxLength(Source.MaxStorageKeyLength);

        builder.Property(source => source.Label)
            .HasMaxLength(Source.MaxLabelLength);

        builder.Property(source => source.ContextNote)
            .HasMaxLength(Source.MaxContextNoteLength);

        builder.Property(source => source.ExternalId)
            .HasMaxLength(Source.MaxExternalIdLength);

        builder.Property(source => source.Language)
            .HasMaxLength(Source.MaxLanguageLength)
            .IsRequired();

        builder.Property(source => source.MediaType)
            .HasMaxLength(Source.MaxMediaTypeLength);

        builder.Property(source => source.SizeBytes);

        builder.Property(source => source.Checksum)
            .HasMaxLength(Source.MaxChecksumLength)
            .IsRequired();

        builder.Property(source => source.MetadataJson)
            .HasMaxLength(Source.MaxMetadataLength);

        builder.Property(source => source.UploadStatus)
            .HasDefaultValue(SourceUploadStatus.None)
            .IsRequired();

        builder.Property(source => source.TenantId)
            .IsRequired();

        builder.HasIndex(source => new { source.TenantId, source.StorageKey })
            .IsUnique()
            .HasFilter("\"StorageKey\" IS NOT NULL");

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
