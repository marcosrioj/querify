using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Broadcast.Common.Persistence.BroadcastDb.Configurations;

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
