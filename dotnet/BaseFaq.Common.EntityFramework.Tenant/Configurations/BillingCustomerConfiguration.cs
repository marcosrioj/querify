using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Tenant.Configurations;

public sealed class BillingCustomerConfiguration : BaseConfiguration<BillingCustomer>
{
    public override void Configure(EntityTypeBuilder<BillingCustomer> builder)
    {
        base.Configure(builder);

        builder.ToTable("BillingCustomers");

        builder.Property(entry => entry.TenantId)
            .IsRequired();

        builder.Property(entry => entry.Provider)
            .IsRequired();

        builder.Property(entry => entry.ExternalCustomerId)
            .IsRequired()
            .HasMaxLength(BillingCustomer.MaxExternalCustomerIdLength);

        builder.Property(entry => entry.Email)
            .HasMaxLength(BillingCustomer.MaxEmailLength);

        builder.Property(entry => entry.CountryCode)
            .HasMaxLength(BillingCustomer.MaxCountryCodeLength);

        builder.HasIndex(entry => new { entry.Provider, entry.ExternalCustomerId })
            .IsUnique()
            .HasDatabaseName("IX_BillingCustomer_Provider_ExternalCustomerId");

        builder.HasIndex(entry => new { entry.TenantId, entry.Provider })
            .HasDatabaseName("IX_BillingCustomer_TenantId_Provider");
    }
}
