using Querify.Models.Tenant.Enums;

namespace Querify.Tenant.Worker.Business.Billing.Models;

public sealed record BillingWebhookEvent
{
    public required Guid InboxId { get; init; }
    public required BillingProviderType Provider { get; init; }
    public required BillingWebhookEventKind Kind { get; init; }
    public required string ExternalEventId { get; init; }
    public required string EventType { get; init; }
    public Guid? TenantId { get; init; }
    public string? ExternalCheckoutSessionId { get; init; }
    public string? ExternalCustomerId { get; init; }
    public string? CustomerEmail { get; init; }
    public string? ExternalSubscriptionId { get; init; }
    public string? ExternalInvoiceId { get; init; }
    public string? ExternalPaymentId { get; init; }
    public string? ExternalPriceId { get; init; }
    public string? ExternalProductId { get; init; }
    public string? PlanCode { get; init; }
    public BillingIntervalType BillingInterval { get; init; } = BillingIntervalType.Unknown;
    public TenantSubscriptionStatus SubscriptionStatus { get; init; } = TenantSubscriptionStatus.Unknown;
    public BillingInvoiceStatus InvoiceStatus { get; init; } = BillingInvoiceStatus.Unknown;
    public BillingPaymentStatus PaymentStatus { get; init; } = BillingPaymentStatus.Unknown;
    public string? Currency { get; init; }
    public string? CountryCode { get; init; }
    public long? AmountMinor { get; init; }
    public DateTime? TrialEndsAtUtc { get; init; }
    public DateTime? CurrentPeriodStartUtc { get; init; }
    public DateTime? CurrentPeriodEndUtc { get; init; }
    public DateTime? DueDateUtc { get; init; }
    public DateTime? PaidAtUtc { get; init; }
    public bool CancelAtPeriodEnd { get; init; }
    public DateTime? CancelledAtUtc { get; init; }
    public string? HostedInvoiceUrl { get; init; }
    public string? PdfUrl { get; init; }
    public string? PaymentMethod { get; init; }
    public string? FailureCode { get; init; }
    public string? FailureMessage { get; init; }
    public DateTime? EventCreatedAtUtc { get; init; }
    public bool IsLiveMode { get; init; }
    public string? ProviderAccountId { get; init; }
    public required string RawPayloadJson { get; init; }
    public required string RawObjectJson { get; init; }
}
