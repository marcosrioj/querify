using Querify.Common.EntityFramework.Core.Configurations;
using Querify.Common.EntityFramework.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Querify.Common.EntityFramework.Tenant.Configurations;

public sealed class BillingInvoiceConfiguration : BaseConfiguration<BillingInvoice>
{
    public override void Configure(EntityTypeBuilder<BillingInvoice> builder)
    {
        base.Configure(builder);

        builder.ToTable("BillingInvoices");

        builder.Property(entry => entry.TenantId)
            .IsRequired();

        builder.Property(entry => entry.Provider)
            .IsRequired();

        builder.Property(entry => entry.ExternalInvoiceId)
            .IsRequired()
            .HasMaxLength(BillingInvoice.MaxExternalInvoiceIdLength);

        builder.Property(entry => entry.Currency)
            .IsRequired()
            .HasMaxLength(BillingInvoice.MaxCurrencyLength);

        builder.Property(entry => entry.Status)
            .IsRequired();

        builder.Property(entry => entry.HostedUrl)
            .HasMaxLength(BillingInvoice.MaxHostedUrlLength);

        builder.Property(entry => entry.PdfUrl)
            .HasMaxLength(BillingInvoice.MaxPdfUrlLength);

        builder.Property(entry => entry.RawSnapshotJson)
            .HasColumnType("text");

        builder.HasOne(entry => entry.TenantSubscription)
            .WithMany(entry => entry.Invoices)
            .HasForeignKey(entry => entry.TenantSubscriptionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(entry => new { entry.Provider, entry.ExternalInvoiceId })
            .IsUnique()
            .HasDatabaseName("IX_BillingInvoice_Provider_ExternalInvoiceId");

        builder.HasIndex(entry => new { entry.TenantId, entry.Status, entry.PaidAtUtc })
            .HasDatabaseName("IX_BillingInvoice_TenantId_Status_PaidAtUtc");
    }
}
