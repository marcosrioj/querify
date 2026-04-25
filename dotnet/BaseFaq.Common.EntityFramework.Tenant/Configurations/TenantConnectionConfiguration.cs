using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Tenant.Configurations;

public class TenantConnectionConfiguration : BaseConfiguration<TenantConnection>
{
    public override void Configure(EntityTypeBuilder<TenantConnection> builder)
    {
        base.Configure(builder);

        builder.ToTable("TenantConnections");

        builder.Property(p => p.ConnectionString)
            .IsRequired()
            .HasMaxLength(TenantConnection.MaxConnectionStringLength);

        builder.Property(p => p.Module)
            .HasColumnName("App")
            .IsRequired();

        builder.Property(p => p.IsCurrent)
            .IsRequired();

        builder.HasIndex(p => new { p.Module, p.IsCurrent })
            .HasDatabaseName("IX_TenantConnection_App_IsCurrent");
    }
}
