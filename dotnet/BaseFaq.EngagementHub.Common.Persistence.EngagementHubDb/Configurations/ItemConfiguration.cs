using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.EngagementHub.Common.Persistence.EngagementHubDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.EngagementHub.Common.Persistence.EngagementHubDb.Configurations;

public class ItemConfiguration : BaseConfiguration<Item>
{
    public override void Configure(EntityTypeBuilder<Item> builder)
    {
        base.Configure(builder);

        builder.ToTable("Items");

        builder.Property(item => item.TenantId).IsRequired();
        builder.Property(item => item.ThreadId).IsRequired();
        builder.Property(item => item.Body).HasMaxLength(Item.MaxBodyLength).IsRequired();
        builder.Property(item => item.CapturedAtUtc).IsRequired();

        builder.HasIndex(item => new { item.ThreadId, item.CapturedAtUtc })
            .HasDatabaseName("IX_Item_ThreadId_CapturedAtUtc");
    }
}
