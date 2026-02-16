using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Tenant.Configurations;

public sealed class AiProviderConfiguration : BaseConfiguration<AiProvider>
{
    public override void Configure(EntityTypeBuilder<AiProvider> builder)
    {
        base.Configure(builder);

        builder.ToTable("AiProviders");

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(AiProvider.MaxProviderLength);

        builder.Property(x => x.Model)
            .IsRequired()
            .HasMaxLength(AiProvider.MaxModelLength);

        builder.Property(x => x.Prompt)
            .IsRequired()
            .HasMaxLength(AiProvider.MaxPromptLength);

        builder.Property(x => x.Command)
            .IsRequired();

        builder.HasIndex(x => new { x.Command, x.Provider, x.Model })
            .IsUnique()
            .HasDatabaseName("IX_AiProvider_Command_Provider_Model");
    }
}