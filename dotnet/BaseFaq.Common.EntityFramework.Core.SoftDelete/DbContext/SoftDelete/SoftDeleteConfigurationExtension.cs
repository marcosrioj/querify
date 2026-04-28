using BaseFaq.Common.EntityFramework.Core.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Core.SoftDelete.DbContext.SoftDelete;

public static class SoftDeleteConfigurationExtension
{
    public static EntityTypeBuilder<TEntity> ConfigureSoftDelete<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, ISoftDelete
    {
        builder.HasIndex(entity => entity.IsDeleted)
            .HasDatabaseName($"IX_{typeof(TEntity).Name}_IsDeleted");

        builder.Property(entity => entity.IsDeleted);

        return builder;
    }
}
