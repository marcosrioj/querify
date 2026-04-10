using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Common.EntityFramework.Tenant.Entities;

public class BillingInvoice : BaseEntity, IMustHaveTenant
{
    public const int MaxExternalInvoiceIdLength = 255;
    public const int MaxCurrencyLength = 8;
    public const int MaxHostedUrlLength = 2048;
    public const int MaxPdfUrlLength = 2048;

    public Guid? TenantSubscriptionId { get; set; }
    public TenantSubscription? TenantSubscription { get; set; }

    public required Guid TenantId { get; set; }
    public BillingProviderType Provider { get; set; } = BillingProviderType.Unknown;
    public required string ExternalInvoiceId { get; set; }
    public long AmountMinor { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime? DueDateUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public BillingInvoiceStatus Status { get; set; } = BillingInvoiceStatus.Unknown;
    public string? HostedUrl { get; set; }
    public string? PdfUrl { get; set; }
    public DateTime? LastEventCreatedAtUtc { get; set; }
    public string? RawSnapshotJson { get; set; }
    public ICollection<BillingPayment> Payments { get; set; } = [];
}
