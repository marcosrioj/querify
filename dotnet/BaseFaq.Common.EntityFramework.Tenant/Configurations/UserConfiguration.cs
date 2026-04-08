using BaseFaq.Common.EntityFramework.Core.Configurations;
using BaseFaq.Common.EntityFramework.Tenant.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BaseFaq.Common.EntityFramework.Tenant.Configurations;

public class UserConfiguration : BaseConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("Users");

        builder.Property(p => p.GivenName)
            .IsRequired()
            .HasMaxLength(User.MaxGivenNameLength);

        builder.Property(p => p.SurName)
            .HasMaxLength(User.MaxSurNameLength);

        builder.Property(p => p.Email)
            .IsRequired()
            .HasMaxLength(User.MaxEmailLength);

        builder.Property(p => p.ExternalId)
            .IsRequired()
            .HasMaxLength(User.MaxExternalIdLength);

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(User.MaxPhoneNumberLength);

        builder.Property(p => p.Language)
            .HasMaxLength(User.MaxLanguageLength);

        builder.Property(p => p.TimeZone)
            .HasMaxLength(User.MaxTimeZoneLength);

        builder.Property(p => p.Role)
            .IsRequired();

        builder.HasIndex(p => p.Email)
            .HasDatabaseName("IX_User_Email");

        builder.HasIndex(p => new { p.ExternalId })
            .IsUnique()
            .HasDatabaseName("IX_User_ExternalId");
    }
}
