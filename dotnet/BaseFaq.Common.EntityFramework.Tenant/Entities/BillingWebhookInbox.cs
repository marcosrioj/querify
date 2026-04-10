using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Common.EntityFramework.Tenant.Enums;

namespace BaseFaq.Common.EntityFramework.Tenant.Entities;

public class BillingWebhookInbox : BaseEntity
{
    public const int MaxProviderLength = 64;
    public const int MaxEventIdLength = 255;
    public const int MaxEventTypeLength = 255;
    public const int MaxSignatureLength = 512;
    public const int MaxLastErrorLength = 2048;

    public required string Provider { get; set; }
    public required string EventId { get; set; }
    public required string EventType { get; set; }
    public required string PayloadJson { get; set; }
    public string? Signature { get; set; }
    public DateTime ReceivedDateUtc { get; set; } = DateTime.UtcNow;
    public ControlPlaneMessageStatus Status { get; set; } = ControlPlaneMessageStatus.Pending;
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptDateUtc { get; set; }
    public DateTime? NextAttemptDateUtc { get; set; }
    public DateTime? ProcessedDateUtc { get; set; }
    public DateTime? LockedUntilDateUtc { get; set; }
    public Guid? ProcessingToken { get; set; }
    public string? LastError { get; set; }
}
