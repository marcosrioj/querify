using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Tenant.Configurations;

public sealed class TenantSubscriptionConfiguration : BaseConfiguration<TenantSubscription>
{
    public override void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        base.Configure(builder);

        builder.ToTable("TenantSubscriptions");

        builder.Property(entry => entry.TenantId)
            .IsRequired();

        builder.Property(entry => entry.PlanCode)
            .HasMaxLength(TenantSubscription.MaxPlanCodeLength);

        builder.Property(entry => entry.Currency)
            .HasMaxLength(TenantSubscription.MaxCurrencyLength);

        builder.Property(entry => entry.CountryCode)
            .HasMaxLength(TenantSubscription.MaxCountryCodeLength);

        builder.Property(entry => entry.BillingInterval)
            .IsRequired();

        builder.Property(entry => entry.Status)
            .IsRequired();

        builder.Property(entry => entry.DefaultProvider)
            .IsRequired();

        builder.HasIndex(entry => entry.TenantId)
            .IsUnique()
            .HasDatabaseName("IX_TenantSubscription_TenantId");

        builder.HasIndex(entry => new { entry.Status, entry.CurrentPeriodEndUtc })
            .HasDatabaseName("IX_TenantSubscription_Status_CurrentPeriodEndUtc");
    }
}
