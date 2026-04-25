using BaseFaq.Common.EntityFramework.Core.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreadEntity = BaseFaq.EngagementHub.Common.Persistence.EngagementHubDb.Entities.Thread;

namespace BaseFaq.EngagementHub.Common.Persistence.EngagementHubDb.Configurations;

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
