using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Tenant.Configurations;

public sealed class EmailOutboxConfiguration : BaseConfiguration<EmailOutbox>
{
    public override void Configure(EntityTypeBuilder<EmailOutbox> builder)
    {
        base.Configure(builder);

        builder.ToTable("EmailOutboxes");

        builder.Property(entry => entry.RecipientEmail)
            .IsRequired()
            .HasMaxLength(EmailOutbox.MaxRecipientEmailLength);

        builder.Property(entry => entry.Subject)
            .IsRequired()
            .HasMaxLength(EmailOutbox.MaxSubjectLength);

        builder.Property(entry => entry.HtmlBody)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(entry => entry.TextBody)
            .HasColumnType("text");

        builder.Property(entry => entry.FromEmail)
            .HasMaxLength(EmailOutbox.MaxFromEmailLength);

        builder.Property(entry => entry.FromName)
            .HasMaxLength(EmailOutbox.MaxFromNameLength);

        builder.Property(entry => entry.Status)
            .IsRequired();

        builder.Property(entry => entry.AttemptCount)
            .IsRequired();

        builder.Property(entry => entry.QueuedDateUtc)
            .IsRequired();

        builder.Property(entry => entry.LastError)
            .HasMaxLength(EmailOutbox.MaxLastErrorLength);

        builder.HasIndex(entry => new { entry.Status, entry.NextAttemptDateUtc, entry.LockedUntilDateUtc })
            .HasDatabaseName("IX_EmailOutbox_Status_NextAttempt_LockedUntil");

        builder.HasIndex(entry => entry.QueuedDateUtc)
            .HasDatabaseName("IX_EmailOutbox_QueuedDateUtc");
    }
}
