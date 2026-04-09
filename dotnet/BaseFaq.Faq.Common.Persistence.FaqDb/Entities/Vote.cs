using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.Faq.Common.Persistence.FaqDb.Entities;

public class Vote : BaseEntity, IMustHaveTenant
{
    public const int MaxUserPrintLength = 250;
    public const int MaxIpLength = 100;
    public const int MaxUserAgentLength = 1000;

    /// <summary>
    /// User fingerprint used to identify the feedback submitter.
    /// Helps prevent duplicate or abusive submissions.
    /// </summary>
    public required string UserPrint { get; set; }

    /// <summary>
    /// IP address of the feedback submitter.
    /// Used for auditing and abuse detection.
    /// </summary>
    public required string Ip { get; set; }

    /// <summary>
    /// User agent string of the client that submitted the feedback.
    /// </summary>
    public required string UserAgent { get; set; }

    public required Guid TenantId { get; set; }

    public required Guid FaqItemAnswerId { get; set; }
    public FaqItemAnswer FaqItemAnswer { get; set; } = null!;
}
