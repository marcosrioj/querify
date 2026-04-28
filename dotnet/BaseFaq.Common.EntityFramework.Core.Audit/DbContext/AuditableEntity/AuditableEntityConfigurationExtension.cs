using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Core.Audit.DbContext.AuditableEntity;

public static class AuditableEntityConfigurationExtension
{
    public static EntityTypeBuilder<TEntity> ConfigureAuditFields<TEntity>(
        this EntityTypeBuilder<TEntity> builder)
        where TEntity : Entities.AuditableEntity
    {
        builder.Property(entity => entity.DeletedBy)
            .IsRequired(false);

        builder.Property(entity => entity.DeletedDate)
            .IsRequired(false);

        builder.Property(entity => entity.CreatedBy)
            .IsRequired(false);

        builder.Property(entity => entity.CreatedDate)
            .IsRequired(false);

        builder.Property(entity => entity.UpdatedBy)
            .IsRequired(false);

        builder.Property(entity => entity.UpdatedDate)
            .IsRequired(false);

        return builder;
    }
}
