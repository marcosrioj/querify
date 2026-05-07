using System.ComponentModel.DataAnnotations;

namespace Querify.QnA.Worker.Business.Source.Options;

public sealed class PendingSourceUploadExpiryOptions
{
    public const string SectionName = "QnAWorker:PendingSourceUploadExpiry";

    public bool Enabled { get; set; } = true;

    [Range(60, int.MaxValue)]
    public int PollingIntervalSeconds { get; set; } = 3600;

    public TimeSpan PollingInterval => TimeSpan.FromSeconds(PollingIntervalSeconds);
}
