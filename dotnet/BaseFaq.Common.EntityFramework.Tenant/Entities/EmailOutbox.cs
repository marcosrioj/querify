using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Common.EntityFramework.Tenant.Enums;

namespace BaseFaq.Common.EntityFramework.Tenant.Entities;

public class EmailOutbox : BaseEntity
{
    public const int MaxRecipientEmailLength = 320;
    public const int MaxSubjectLength = 255;
    public const int MaxFromEmailLength = 320;
    public const int MaxFromNameLength = 128;
    public const int MaxLastErrorLength = 2048;

    public required string RecipientEmail { get; set; }
    public required string Subject { get; set; }
    public required string HtmlBody { get; set; }
    public string? TextBody { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
    public DateTime QueuedDateUtc { get; set; } = DateTime.UtcNow;
    public ControlPlaneMessageStatus Status { get; set; } = ControlPlaneMessageStatus.Pending;
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptDateUtc { get; set; }
    public DateTime? NextAttemptDateUtc { get; set; }
    public DateTime? ProcessedDateUtc { get; set; }
    public DateTime? LockedUntilDateUtc { get; set; }
    public Guid? ProcessingToken { get; set; }
    public string? LastError { get; set; }
}
