using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.Faq.Enums;

namespace BaseFaq.Faq.Common.Persistence.FaqDb.Entities;

public class Faq : BaseEntity, IMustHaveTenant
{
    public const int MaxNameLength = 200;
    public const int MaxLanguageLength = 50;

    /// <summary>
    /// Display name of the FAQ.
    /// Represents the main title shown to users and consumers.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Language or locale of the FAQ content (e.g. "en-CA", "pt-BR").
    /// </summary>
    public required string Language { get; set; }

    /// <summary>
    /// Current lifecycle status of the FAQ (draft, published, archived, etc.).
    /// </summary>
    public required FaqStatus Status { get; set; }

    public required Guid TenantId { get; set; }

    public ICollection<FaqItem> Items { get; set; } = [];

    public ICollection<FaqContentRef> ContentRefs { get; set; } = [];
    public ICollection<FaqTag> Tags { get; set; } = [];
}