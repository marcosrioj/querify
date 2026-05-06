using Querify.Common.EntityFramework.Core.Configurations;
using Querify.Common.EntityFramework.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Querify.Common.EntityFramework.Tenant.Configurations;

public sealed class TenantEntitlementSnapshotConfiguration : BaseConfiguration<TenantEntitlementSnapshot>
{
    public override void Configure(EntityTypeBuilder<TenantEntitlementSnapshot> builder)
    {
        base.Configure(builder);

        builder.ToTable("TenantEntitlementSnapshots");

        builder.Property(entry => entry.TenantId)
            .IsRequired();

        builder.Property(entry => entry.PlanCode)
            .HasMaxLength(TenantEntitlementSnapshot.MaxPlanCodeLength);

        builder.Property(entry => entry.SubscriptionStatus)
            .IsRequired();

        builder.Property(entry => entry.FeatureJson)
            .HasColumnType("text");

        builder.HasIndex(entry => entry.TenantId)
            .IsUnique()
            .HasDatabaseName("IX_TenantEntitlementSnapshot_TenantId");

        builder.HasIndex(entry => new { entry.IsActive, entry.IsInGracePeriod, entry.EffectiveUntilUtc })
            .HasDatabaseName("IX_TenantEntitlementSnapshot_IsActive_IsInGracePeriod_EffectiveUntilUtc");
    }
}
