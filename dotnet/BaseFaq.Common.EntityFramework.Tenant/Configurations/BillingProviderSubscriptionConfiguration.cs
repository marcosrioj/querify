using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Tenant.Configurations;

public sealed class BillingProviderSubscriptionConfiguration : BaseConfiguration<BillingProviderSubscription>
{
    public override void Configure(EntityTypeBuilder<BillingProviderSubscription> builder)
    {
        base.Configure(builder);

        builder.ToTable("BillingProviderSubscriptions");

        builder.Property(entry => entry.TenantSubscriptionId)
            .IsRequired();

        builder.Property(entry => entry.TenantId)
            .IsRequired();

        builder.Property(entry => entry.Provider)
            .IsRequired();

        builder.Property(entry => entry.ExternalSubscriptionId)
            .IsRequired()
            .HasMaxLength(BillingProviderSubscription.MaxExternalSubscriptionIdLength);

        builder.Property(entry => entry.ExternalPriceId)
            .HasMaxLength(BillingProviderSubscription.MaxExternalPriceIdLength);

        builder.Property(entry => entry.ExternalProductId)
            .HasMaxLength(BillingProviderSubscription.MaxExternalProductIdLength);

        builder.Property(entry => entry.Status)
            .IsRequired();

        builder.Property(entry => entry.RawSnapshotJson)
            .HasColumnType("text");

        builder.HasOne(entry => entry.TenantSubscription)
            .WithMany(entry => entry.ProviderSubscriptions)
            .HasForeignKey(entry => entry.TenantSubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(entry => new { entry.Provider, entry.ExternalSubscriptionId })
            .IsUnique()
            .HasDatabaseName("IX_BillingProviderSubscription_Provider_ExternalSubscriptionId");

        builder.HasIndex(entry => new { entry.TenantId, entry.TenantSubscriptionId })
            .HasDatabaseName("IX_BillingProviderSubscription_TenantId_TenantSubscriptionId");
    }
}
