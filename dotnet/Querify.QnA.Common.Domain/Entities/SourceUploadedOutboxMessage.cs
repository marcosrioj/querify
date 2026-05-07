using Querify.Common.EntityFramework.Core.Abstractions;
using Querify.Common.EntityFramework.Core.Entities;
using Querify.Models.QnA.Enums;

namespace Querify.QnA.Common.Domain.Entities;

public class SourceUploadedOutboxMessage : BaseEntity, IMustHaveTenant
{
    public const int MaxStorageKeyLength = 1000;
    public const int MaxClientChecksumLength = 256;
    public const int MaxLastErrorLength = 2048;

    public required Guid TenantId { get; set; }
    public required Guid SourceId { get; set; }
    public required string StorageKey { get; set; }
    public string? ClientChecksum { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public SourceUploadOutboxStatus Status { get; set; } = SourceUploadOutboxStatus.Pending;
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptDateUtc { get; set; }
    public DateTime? NextAttemptDateUtc { get; set; }
    public DateTime? ProcessedDateUtc { get; set; }
    public DateTime? LockedUntilDateUtc { get; set; }
    public Guid? ProcessingToken { get; set; }
    public string? LastError { get; set; }
}
