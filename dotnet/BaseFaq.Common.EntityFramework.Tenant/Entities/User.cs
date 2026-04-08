using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.User.Enums;

namespace BaseFaq.Common.EntityFramework.Tenant.Entities;

public class User : BaseEntity
{
    public const int MaxGivenNameLength = 100;
    public const int MaxSurNameLength = 100;
    public const int MaxEmailLength = 200;
    public const int MaxPhoneNumberLength = 200;
    public const int MaxExternalIdLength = 200;
    public const int MaxTimeZoneLength = 100;

    public required string GivenName { get; set; }
    public string? SurName { get; set; }
    public required string Email { get; set; }
    public required string ExternalId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? TimeZone { get; set; }
    public UserRoleType Role { get; set; }
    public ICollection<TenantUser> TenantUsers { get; set; } = [];
}
