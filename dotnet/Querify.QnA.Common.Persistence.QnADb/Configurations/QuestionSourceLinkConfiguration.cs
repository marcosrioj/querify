using Querify.Common.EntityFramework.Core.Configurations;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Querify.QnA.Common.Persistence.QnADb.Configurations;

public class QuestionSourceLinkConfiguration : BaseConfiguration<QuestionSourceLink>
{
    public override void Configure(EntityTypeBuilder<QuestionSourceLink> builder)
    {
        base.Configure(builder);

        builder.ToTable("QuestionSourceLinks");

        builder.Property(link => link.TenantId)
            .IsRequired();

        builder.Property(link => link.QuestionId)
            .IsRequired();

        builder.Property(link => link.SourceId)
            .IsRequired();

        builder.HasIndex(link => new { link.QuestionId, link.SourceId, link.Role, link.Order })
            .HasDatabaseName("IX_QuestionSourceLink_QuestionId_SourceId_Role_Order")
            .IsUnique();
    }
}