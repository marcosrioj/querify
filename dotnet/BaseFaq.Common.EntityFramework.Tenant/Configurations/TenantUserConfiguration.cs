using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Tenant.Configurations;

public class TenantUserConfiguration : BaseConfiguration<TenantUser>
{
    public override void Configure(EntityTypeBuilder<TenantUser> builder)
    {
        base.Configure(builder);

        builder.ToTable("TenantUsers");

        builder.Property(p => p.TenantId)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.Role)
            .IsRequired();

        builder.HasOne(p => p.Tenant)
            .WithMany(p => p.TenantUsers)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.User)
            .WithMany(p => p.TenantUsers)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.TenantId, p.UserId })
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false")
            .HasDatabaseName("IX_TenantUser_TenantId_UserId");

        builder.HasIndex(p => new { p.TenantId, p.Role })
            .HasDatabaseName("IX_TenantUser_TenantId_Role");

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("IX_TenantUser_UserId");
    }
}
