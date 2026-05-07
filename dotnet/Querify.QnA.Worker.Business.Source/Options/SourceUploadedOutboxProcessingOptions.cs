using System.ComponentModel.DataAnnotations;

namespace Querify.QnA.Worker.Business.Source.Options;

public sealed class SourceUploadedOutboxProcessingOptions
{
    public const string SectionName = "QnAWorker:SourceUploadedOutbox";

    public bool Enabled { get; set; } = true;

    [Range(1, int.MaxValue)]
    public int PollingIntervalSeconds { get; set; } = 10;

    [Range(1, 500)]
    public int BatchSize { get; set; } = 20;

    [Range(1, int.MaxValue)]
    public int LeaseDurationSeconds { get; set; } = 60;

    [Range(1, int.MaxValue)]
    public int FailureBackoffSeconds { get; set; } = 30;

    [Range(1, 100)]
    public int MaxRetryCount { get; set; } = 5;

    public TimeSpan PollingInterval => TimeSpan.FromSeconds(PollingIntervalSeconds);
    public TimeSpan LeaseDuration => TimeSpan.FromSeconds(LeaseDurationSeconds);
    public TimeSpan FailureBackoff => TimeSpan.FromSeconds(FailureBackoffSeconds);
}
