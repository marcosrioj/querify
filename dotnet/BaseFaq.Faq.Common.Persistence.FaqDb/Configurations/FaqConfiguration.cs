using BaseFaq.Common.EntityFramework.Core.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Faq.Common.Persistence.FaqDb.Configurations;

public class FaqConfiguration : BaseConfiguration<Entities.Faq>
{
    public override void Configure(EntityTypeBuilder<Entities.Faq> builder)
    {
        base.Configure(builder);

        builder.ToTable("Faqs");

        builder.Property(p => p.Name)
            .HasMaxLength(Entities.Faq.MaxNameLength)
            .IsRequired();

        builder.Property(p => p.Language)
            .HasMaxLength(Entities.Faq.MaxLanguageLength)
            .IsRequired();

        builder.Property(p => p.Status)
            .IsRequired();

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.HasMany(p => p.Items)
            .WithOne(item => item.Faq)
            .HasForeignKey(item => item.FaqId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Tags)
            .WithOne(tag => tag.Faq)
            .HasForeignKey(tag => tag.FaqId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.ContentRefs)
            .WithOne(contentRef => contentRef.Faq)
            .HasForeignKey(contentRef => contentRef.FaqId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
