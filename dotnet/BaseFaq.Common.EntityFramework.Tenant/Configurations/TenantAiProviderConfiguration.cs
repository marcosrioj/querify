using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Tenant.Configurations;

public sealed class TenantAiProviderConfiguration : BaseConfiguration<TenantAiProvider>
{
    public override void Configure(EntityTypeBuilder<TenantAiProvider> builder)
    {
        base.Configure(builder);

        builder.ToTable("TenantAiProviders");

        builder.Property(x => x.TenantId)
            .IsRequired();

        builder.Property(x => x.AiProviderId)
            .IsRequired();

        builder.Property(x => x.AiProviderKey)
            .IsRequired()
            .HasMaxLength(TenantAiProvider.MaxAiProviderKeyLength);

        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.AiProviders)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AiProvider)
            .WithMany()
            .HasForeignKey(x => x.AiProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.AiProviderId })
            .IsUnique()
            .HasDatabaseName("IX_TenantAiProvider_TenantId_AiProviderId");
    }
}