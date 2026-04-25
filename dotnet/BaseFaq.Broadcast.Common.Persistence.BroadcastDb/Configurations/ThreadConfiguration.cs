using BaseFaq.Common.EntityFramework.Core.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreadEntity = BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Entities.Thread;

namespace BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Configurations;

public class ThreadConfiguration : BaseConfiguration<ThreadEntity>
{
    public override void Configure(EntityTypeBuilder<ThreadEntity> builder)
    {
        base.Configure(builder);

        builder.ToTable("Threads");

        builder.Property(thread => thread.TenantId).IsRequired();
        builder.Property(thread => thread.Title).HasMaxLength(ThreadEntity.MaxTitleLength);

        builder.HasMany(thread => thread.Items)
            .WithOne(item => item.Thread)
            .HasForeignKey(item => item.ThreadId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
