using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Common.EntityFramework.Tenant.Entities;

public class BillingCustomer : BaseEntity, IMustHaveTenant
{
    public const int MaxExternalCustomerIdLength = 255;
    public const int MaxEmailLength = 320;
    public const int MaxCountryCodeLength = 8;

    public required Guid TenantId { get; set; }
    public BillingProviderType Provider { get; set; } = BillingProviderType.Unknown;
    public required string ExternalCustomerId { get; set; }
    public string? Email { get; set; }
    public string? CountryCode { get; set; }
    public DateTime? LastEventCreatedAtUtc { get; set; }
}
