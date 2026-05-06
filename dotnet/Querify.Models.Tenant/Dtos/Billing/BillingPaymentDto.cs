using Querify.Models.Tenant.Enums;

namespace Querify.Models.Tenant.Dtos.Billing;

public sealed class BillingPaymentDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? BillingInvoiceId { get; set; }
    public BillingProviderType Provider { get; set; }
    public string? ExternalPaymentId { get; set; }
    public string? Method { get; set; }
    public long AmountMinor { get; set; }
    public string Currency { get; set; } = string.Empty;
    public BillingPaymentStatus Status { get; set; }
    public string? FailureCode { get; set; }
    public string? FailureMessage { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public DateTime? CreatedDateUtc { get; set; }
    public DateTime? UpdatedDateUtc { get; set; }
}
