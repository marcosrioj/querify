using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.EntityFramework.Core.Entities;
using Querify.Common.EntityFramework.Tenant.Enums;
using Querify.Models.Tenant.Enums;

namespace Querify.Common.EntityFramework.Tenant.Entities;

public class BillingWebhookInbox : BaseEntity, IMayHaveTenant
{
    public const int MaxExternalEventIdLength = 255;
    public const int MaxEventTypeLength = 255;
    public const int MaxSignatureLength = 512;
    public const int MaxProviderAccountIdLength = 255;
    public const int MaxLastErrorLength = 2048;

    public Guid? TenantId { get; set; }
    public BillingProviderType Provider { get; set; } = BillingProviderType.Unknown;
    public required string ExternalEventId { get; set; }
    public required string EventType { get; set; }
    public required string PayloadJson { get; set; }
    public string? Signature { get; set; }
    public bool SignatureValid { get; set; }
    public bool IsLiveMode { get; set; }
    public string? ProviderAccountId { get; set; }
    public DateTime ReceivedDateUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EventCreatedAtUtc { get; set; }
    public ControlPlaneMessageStatus Status { get; set; } = ControlPlaneMessageStatus.Pending;
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptDateUtc { get; set; }
    public DateTime? NextAttemptDateUtc { get; set; }
    public DateTime? ProcessedDateUtc { get; set; }
    public DateTime? LockedUntilDateUtc { get; set; }
    public Guid? ProcessingToken { get; set; }
    public string? LastError { get; set; }
}
