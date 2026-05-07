using Querify.Common.EntityFramework.Core.Configurations;
using Querify.Models.QnA.Enums;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Querify.QnA.Common.Persistence.QnADb.Configurations;

public sealed class SourceUploadedOutboxMessageConfiguration : BaseConfiguration<SourceUploadedOutboxMessage>
{
    public override void Configure(EntityTypeBuilder<SourceUploadedOutboxMessage> builder)
    {
        base.Configure(builder);

        builder.ToTable("SourceUploadedOutboxMessages");

        builder.Property(message => message.TenantId)
            .IsRequired();

        builder.Property(message => message.SourceId)
            .IsRequired();

        builder.Property(message => message.StorageKey)
            .HasMaxLength(SourceUploadedOutboxMessage.MaxStorageKeyLength)
            .IsRequired();

        builder.Property(message => message.ClientChecksum)
            .HasMaxLength(SourceUploadedOutboxMessage.MaxClientChecksumLength);

        builder.Property(message => message.UploadedAtUtc)
            .IsRequired();

        builder.Property(message => message.Status)
            .HasDefaultValue(SourceUploadOutboxStatus.Pending)
            .IsRequired();

        builder.Property(message => message.AttemptCount)
            .IsRequired();

        builder.Property(message => message.LastError)
            .HasMaxLength(SourceUploadedOutboxMessage.MaxLastErrorLength);

        builder.HasIndex(message => new { message.Status, message.NextAttemptDateUtc, message.LockedUntilDateUtc })
            .HasDatabaseName("IX_SourceUploadedOutbox_Status_NextAttempt_LockedUntil");

        builder.HasIndex(message => message.UploadedAtUtc)
            .HasDatabaseName("IX_SourceUploadedOutbox_UploadedAtUtc");
    }
}
