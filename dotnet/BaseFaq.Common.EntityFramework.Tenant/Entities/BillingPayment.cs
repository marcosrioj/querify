using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Common.EntityFramework.Tenant.Entities;

public class BillingPayment : BaseEntity, IMustHaveTenant
{
    public const int MaxExternalPaymentIdLength = 255;
    public const int MaxMethodLength = 128;
    public const int MaxCurrencyLength = 8;
    public const int MaxFailureCodeLength = 128;
    public const int MaxFailureMessageLength = 2048;

    public Guid? BillingInvoiceId { get; set; }
    public BillingInvoice? BillingInvoice { get; set; }

    public required Guid TenantId { get; set; }
    public BillingProviderType Provider { get; set; } = BillingProviderType.Unknown;
    public string? ExternalPaymentId { get; set; }
    public string? Method { get; set; }
    public long AmountMinor { get; set; }
    public string Currency { get; set; } = string.Empty;
    public BillingPaymentStatus Status { get; set; } = BillingPaymentStatus.Unknown;
    public string? FailureCode { get; set; }
    public string? FailureMessage { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public DateTime? LastEventCreatedAtUtc { get; set; }
    public string? RawSnapshotJson { get; set; }
}
