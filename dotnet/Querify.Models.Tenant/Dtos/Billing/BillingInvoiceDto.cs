using Querify.Models.Tenant.Enums;

namespace Querify.Models.Tenant.Dtos.Billing;

public sealed class BillingInvoiceDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? TenantSubscriptionId { get; set; }
    public BillingProviderType Provider { get; set; }
    public string ExternalInvoiceId { get; set; } = string.Empty;
    public long AmountMinor { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime? DueDateUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public BillingInvoiceStatus Status { get; set; }
    public string? HostedUrl { get; set; }
    public string? PdfUrl { get; set; }
    public DateTime? CreatedDateUtc { get; set; }
    public DateTime? UpdatedDateUtc { get; set; }
}
