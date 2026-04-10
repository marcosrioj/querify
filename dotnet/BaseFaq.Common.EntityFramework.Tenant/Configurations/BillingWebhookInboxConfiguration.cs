using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Tenant.Configurations;

public sealed class BillingWebhookInboxConfiguration : BaseConfiguration<BillingWebhookInbox>
{
    public override void Configure(EntityTypeBuilder<BillingWebhookInbox> builder)
    {
        base.Configure(builder);

        builder.ToTable("BillingWebhookInboxes");

        builder.Property(entry => entry.TenantId);

        builder.Property(entry => entry.Provider)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(entry => entry.ExternalEventId)
            .IsRequired()
            .HasMaxLength(BillingWebhookInbox.MaxExternalEventIdLength);

        builder.Property(entry => entry.EventType)
            .IsRequired()
            .HasMaxLength(BillingWebhookInbox.MaxEventTypeLength);

        builder.Property(entry => entry.PayloadJson)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(entry => entry.Signature)
            .HasMaxLength(BillingWebhookInbox.MaxSignatureLength);

        builder.Property(entry => entry.ProviderAccountId)
            .HasMaxLength(BillingWebhookInbox.MaxProviderAccountIdLength);

        builder.Property(entry => entry.Status)
            .IsRequired();

        builder.Property(entry => entry.AttemptCount)
            .IsRequired();

        builder.Property(entry => entry.ReceivedDateUtc)
            .IsRequired();

        builder.Property(entry => entry.LastError)
            .HasMaxLength(BillingWebhookInbox.MaxLastErrorLength);

        builder.HasIndex(entry => new { entry.Provider, entry.ExternalEventId })
            .IsUnique()
            .HasDatabaseName("IX_BillingWebhookInbox_Provider_ExternalEventId");

        builder.HasIndex(entry => new { entry.Status, entry.NextAttemptDateUtc, entry.LockedUntilDateUtc })
            .HasDatabaseName("IX_BillingWebhookInbox_Status_NextAttempt_LockedUntil");

        builder.HasIndex(entry => entry.ReceivedDateUtc)
            .HasDatabaseName("IX_BillingWebhookInbox_ReceivedDateUtc");
    }
}
