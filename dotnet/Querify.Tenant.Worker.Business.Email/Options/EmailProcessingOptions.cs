using System.ComponentModel.DataAnnotations;

namespace Querify.Tenant.Worker.Business.Email.Options;

public sealed class EmailProcessingOptions
{
    public const string SectionName = "TenantWorker:EmailOutbox";

    public bool Enabled { get; set; } = true;

    [Range(1, 500)]
    public int BatchSize { get; set; } = 25;

    [Range(1, 3600)]
    public int PollingIntervalSeconds { get; set; } = 30;

    [Range(5, 3600)]
    public int LeaseDurationSeconds { get; set; } = 120;

    [Range(5, 86400)]
    public int FailureBackoffSeconds { get; set; } = 60;

    [Range(1, 1000)]
    public int MaxRetryCount { get; set; } = 10;

    public TimeSpan PollingInterval => TimeSpan.FromSeconds(PollingIntervalSeconds);

    public TimeSpan LeaseDuration => TimeSpan.FromSeconds(LeaseDurationSeconds);

    public TimeSpan FailureBackoff => TimeSpan.FromSeconds(FailureBackoffSeconds);
}
