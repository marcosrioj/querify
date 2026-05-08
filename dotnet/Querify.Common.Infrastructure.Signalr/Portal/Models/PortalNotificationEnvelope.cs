namespace Querify.Common.Infrastructure.Signalr.Portal.Models;

public sealed class PortalNotificationEnvelope
{
    public required Guid NotificationId { get; init; }
    public required DateTime OccurredAtUtc { get; init; }
    public required string Type { get; init; }
    public required string Module { get; init; }
    public required Guid TenantId { get; init; }
    public required string ResourceKind { get; init; }
    public required Guid ResourceId { get; init; }
    public required int Version { get; init; }
    public required object Payload { get; init; }
}
