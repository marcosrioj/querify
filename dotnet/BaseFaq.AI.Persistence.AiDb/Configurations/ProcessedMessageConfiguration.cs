using BaseFaq.AI.Persistence.AiDb.Entities;
using BaseFaq.Common.EntityFramework.Core.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.AI.Persistence.AiDb.Configurations;

public class ProcessedMessageConfiguration : BaseConfiguration<ProcessedMessage>
{
    public override void Configure(EntityTypeBuilder<ProcessedMessage> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProcessedMessages");

        builder.Property(p => p.HandlerName)
            .HasMaxLength(ProcessedMessage.MaxHandlerNameLength)
            .IsRequired();

        builder.Property(p => p.MessageId)
            .HasMaxLength(ProcessedMessage.MaxMessageIdLength)
            .IsRequired();

        builder.Property(p => p.ProcessedUtc)
            .IsRequired();

        builder.HasIndex(p => new { p.HandlerName, p.MessageId })
            .HasDatabaseName("IX_ProcessedMessage_HandlerName_MessageId")
            .IsUnique();

        builder.HasIndex(p => p.ProcessedUtc)
            .HasDatabaseName("IX_ProcessedMessage_ProcessedUtc");
    }
}