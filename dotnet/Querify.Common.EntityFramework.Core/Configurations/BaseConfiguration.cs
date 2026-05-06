using Querify.Common.EntityFramework.Core.Audit.DbContext.AuditableEntity;
using Querify.Common.EntityFramework.Core.Entities;
using Querify.Common.EntityFramework.Core.SoftDelete.DbContext.SoftDelete;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Querify.Common.EntityFramework.Core.Configurations;

public abstract class BaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.ConfigureSoftDelete();
        builder.ConfigureAuditFields();
    }
}
