using BaseFaq.Common.EntityFramework.Core.Audit.DbContext.AuditableEntity;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Common.EntityFramework.Core.SoftDelete.DbContext.SoftDelete;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Core.Configurations;

public abstract class BaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ConfigureSoftDelete();
        builder.ConfigureAuditFields();
    }
}
