using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.SupportCopilot.Common.Persistence.SupportCopilotDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.SupportCopilot.Common.Persistence.SupportCopilotDb.Configurations;

public class ConversationConfiguration : BaseConfiguration<Conversation>
{
    public override void Configure(EntityTypeBuilder<Conversation> builder)
    {
        base.Configure(builder);

        builder.ToTable("Conversations");

        builder.Property(conversation => conversation.TenantId).IsRequired();
        builder.Property(conversation => conversation.Subject).HasMaxLength(Conversation.MaxSubjectLength);
        builder.Property(conversation => conversation.StartedAtUtc).IsRequired();

        builder.HasMany(conversation => conversation.Messages)
            .WithOne(message => message.Conversation)
            .HasForeignKey(message => message.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(conversation => new { conversation.TenantId, conversation.StartedAtUtc })
            .HasDatabaseName("IX_Conversation_TenantId_StartedAtUtc");
    }
}
