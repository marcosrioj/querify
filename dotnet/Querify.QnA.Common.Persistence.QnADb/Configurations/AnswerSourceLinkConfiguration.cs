using Querify.Common.EntityFramework.Core.Configurations;
using Querify.QnA.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Querify.QnA.Common.Persistence.QnADb.Configurations;

public class AnswerSourceLinkConfiguration : BaseConfiguration<AnswerSourceLink>
{
    public override void Configure(EntityTypeBuilder<AnswerSourceLink> builder)
    {
        base.Configure(builder);

        builder.ToTable("AnswerSourceLinks");

        builder.Property(link => link.TenantId)
            .IsRequired();

        builder.Property(link => link.AnswerId)
            .IsRequired();

        builder.Property(link => link.SourceId)
            .IsRequired();

        builder.HasIndex(link => new { link.AnswerId, link.SourceId, link.Role, link.Order })
            .HasDatabaseName("IX_AnswerSourceLink_AnswerId_SourceId_Role_Order")
            .IsUnique();
    }
}