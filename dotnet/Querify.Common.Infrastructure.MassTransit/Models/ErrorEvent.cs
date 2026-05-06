namespace Querify.Common.Infrastructure.MassTransit.Models;

public class ErrorEvent
{
    public required string OriginalQueueName { get; set; }
    public required string ErrorQueueName { get; set; }
    public string? MessageId { get; set; }
    public required string ExceptionMessage { get; set; }
    public string? ExceptionStackTrace { get; set; }
    public required DateTime Timestamp { get; set; }
    public Guid? CorrelationId { get; set; }
}