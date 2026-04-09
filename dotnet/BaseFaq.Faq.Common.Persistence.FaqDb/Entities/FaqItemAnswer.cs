using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.Faq.Common.Persistence.FaqDb.Entities;

public class FaqItemAnswer : BaseEntity, IMustHaveTenant
{
    public const int MaxShortAnswerLength = 250;
    public const int MaxAnswerLength = 5000;
    
    /// <summary>
    /// Short answer or summary version of the response.
    /// Useful for previews, lists, or condensed displays.
    /// </summary>
    public required string ShortAnswer { get; set; }

    /// <summary>
    /// Full detailed answer for the question.
    /// </summary>
    public string? Answer { get; set; }

    /// <summary>
    /// Manual ordering value used when sort strategy is manual.
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// Cached score representing user feedback.
    /// Typically derived from associated vote entries.
    /// </summary>
    public int VoteScore { get; set; }

    /// <summary>
    /// Indicates whether this FAQ item answer is active and should be displayed.
    /// </summary>
    public bool IsActive { get; set; }

    public required Guid TenantId { get; set; }

    public required Guid FaqItemId { get; set; }
    public FaqItem FaqItem { get; set; } = null!;

    public ICollection<Vote> Votes { get; set; } = [];
}
