using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.QnA.Common.Persistence.QnADb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Configurations;

public class KnowledgeSourceConfiguration : BaseConfiguration<KnowledgeSource>
{
    public override void Configure(EntityTypeBuilder<KnowledgeSource> builder)
    {
        base.Configure(builder);

        builder.ToTable("KnowledgeSources");

        builder.Property(source => source.Locator)
            .HasMaxLength(KnowledgeSource.MaxLocatorLength)
            .IsRequired();

        builder.Property(source => source.Label)
            .HasMaxLength(KnowledgeSource.MaxLabelLength);

        builder.Property(source => source.Scope)
            .HasMaxLength(KnowledgeSource.MaxScopeLength);

        builder.Property(source => source.SystemName)
            .HasMaxLength(KnowledgeSource.MaxSystemNameLength);

        builder.Property(source => source.ExternalId)
            .HasMaxLength(KnowledgeSource.MaxExternalIdLength);

        builder.Property(source => source.Language)
            .HasMaxLength(KnowledgeSource.MaxLanguageLength);

        builder.Property(source => source.MediaType)
            .HasMaxLength(KnowledgeSource.MaxMediaTypeLength);

        builder.Property(source => source.Checksum)
            .HasMaxLength(KnowledgeSource.MaxChecksumLength);

        builder.Property(source => source.MetadataJson)
            .HasMaxLength(KnowledgeSource.MaxMetadataLength);

        builder.Property(source => source.TenantId)
            .IsRequired();

        builder.HasMany(source => source.QuestionSpaceSources)
            .WithOne(link => link.KnowledgeSource)
            .HasForeignKey(link => link.KnowledgeSourceId)
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
