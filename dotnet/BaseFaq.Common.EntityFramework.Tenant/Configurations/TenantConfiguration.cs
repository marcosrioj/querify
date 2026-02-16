using BaseFaq.Common.EntityFramework.Core.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Tenant.Configurations;

public class TenantConfiguration : BaseConfiguration<Entities.Tenant>
{
    public override void Configure(EntityTypeBuilder<Entities.Tenant> builder)
    {
        base.Configure(builder);

        builder.ToTable("Tenants");

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(Entities.Tenant.MaxSlugLength);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(Entities.Tenant.MaxNameLength);

        builder.Property(p => p.ConnectionString)
            .IsRequired()
            .HasMaxLength(Entities.Tenant.MaxConnectionStringLength);

        builder.Property(p => p.ClientKey)
            .HasMaxLength(Entities.Tenant.MaxClientKeyLength);

        builder.Property(p => p.Edition)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.HasIndex(p => p.Slug)
            .IsUnique()
            .HasDatabaseName("IX_Tenant_Slug");

        builder.HasIndex(p => p.UserId)
            .IsUnique()
            .HasDatabaseName("IX_Tenant_UserId");

        builder.HasIndex(p => p.ClientKey)
            .IsUnique()
            .HasDatabaseName("IX_Tenant_ClientKey");
    }
}