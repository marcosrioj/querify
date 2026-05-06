using Querify.Common.EntityFramework.Core.Configurations;
using Querify.Common.EntityFramework.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Querify.Common.EntityFramework.Tenant.Configurations;

public sealed class BillingPaymentConfiguration : BaseConfiguration<BillingPayment>
{
    public override void Configure(EntityTypeBuilder<BillingPayment> builder)
    {
        base.Configure(builder);

        builder.ToTable("BillingPayments");

        builder.Property(entry => entry.TenantId)
            .IsRequired();

        builder.Property(entry => entry.Provider)
            .IsRequired();

        builder.Property(entry => entry.ExternalPaymentId)
            .HasMaxLength(BillingPayment.MaxExternalPaymentIdLength);

        builder.Property(entry => entry.Method)
            .HasMaxLength(BillingPayment.MaxMethodLength);

        builder.Property(entry => entry.Currency)
            .IsRequired()
            .HasMaxLength(BillingPayment.MaxCurrencyLength);

        builder.Property(entry => entry.Status)
            .IsRequired();

        builder.Property(entry => entry.FailureCode)
            .HasMaxLength(BillingPayment.MaxFailureCodeLength);

        builder.Property(entry => entry.FailureMessage)
            .HasMaxLength(BillingPayment.MaxFailureMessageLength);

        builder.Property(entry => entry.RawSnapshotJson)
            .HasColumnType("text");

        builder.HasOne(entry => entry.BillingInvoice)
            .WithMany(entry => entry.Payments)
            .HasForeignKey(entry => entry.BillingInvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(entry => new { entry.Provider, entry.ExternalPaymentId })
            .IsUnique()
            .HasFilter("\"ExternalPaymentId\" IS NOT NULL")
            .HasDatabaseName("IX_BillingPayment_Provider_ExternalPaymentId");

        builder.HasIndex(entry => new { entry.TenantId, entry.Status, entry.PaidAtUtc })
            .HasDatabaseName("IX_BillingPayment_TenantId_Status_PaidAtUtc");
    }
}
