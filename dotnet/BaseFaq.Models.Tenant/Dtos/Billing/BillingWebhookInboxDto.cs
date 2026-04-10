using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Models.Tenant.Dtos.Billing;

public class BillingWebhookInboxDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public BillingProviderType Provider { get; set; }
    public string ExternalEventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public bool SignatureValid { get; set; }
    public bool IsLiveMode { get; set; }
    public string? ProviderAccountId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public DateTime ReceivedDateUtc { get; set; }
    public DateTime? EventCreatedAtUtc { get; set; }
    public DateTime? LastAttemptDateUtc { get; set; }
    public DateTime? NextAttemptDateUtc { get; set; }
    public DateTime? ProcessedDateUtc { get; set; }
    public string? LastError { get; set; }
}
