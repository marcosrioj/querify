using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Direct.Common.Persistence.DirectDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Direct.Common.Persistence.DirectDb.Configurations;

public class ConversationMessageConfiguration : BaseConfiguration<ConversationMessage>
{
    public override void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        base.Configure(builder);

        builder.ToTable("ConversationMessages");

        builder.Property(message => message.TenantId).IsRequired();
        builder.Property(message => message.ConversationId).IsRequired();
        builder.Property(message => message.Body).HasMaxLength(ConversationMessage.MaxBodyLength).IsRequired();
        builder.Property(message => message.SentAtUtc).IsRequired();

        builder.HasIndex(message => new { message.ConversationId, message.SentAtUtc })
            .HasDatabaseName("IX_ConversationMessage_ConversationId_SentAtUtc");
    }
}
