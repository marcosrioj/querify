using System.ComponentModel.DataAnnotations;

namespace Querify.QnA.Worker.Business.Source.Options;

public sealed class SourceUploadVerificationSweepOptions
{
    public const string SectionName = "QnAWorker:SourceUploadVerification";
    public const string DefaultQueueName = "qna-source-upload";
    public const string DefaultRecurringJobId = "qna-source-upload-verification";

    public bool Enabled { get; set; } = true;

    [Range(1, 1000)]
    public int BatchSize { get; set; } = 20;

    [Required]
    public string CronExpression { get; set; } = "*/1 * * * *";

    [Required]
    [RegularExpression("^[a-z0-9_-]+$")]
    public string QueueName { get; set; } = DefaultQueueName;

    [Required]
    public string RecurringJobId { get; set; } = DefaultRecurringJobId;
}
