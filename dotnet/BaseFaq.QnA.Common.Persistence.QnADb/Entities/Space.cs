using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
///     Represents a Q&amp;A space where questions live, including exposure,
///     moderation, and curation rules.
/// </summary>
public class Space : BaseEntity, IMustHaveTenant
{
    public const int MaxNameLength = 200;
    public const int MaxSlugLength = 160;
    public const int MaxSummaryLength = 2000;
    public const int MaxLanguageLength = 50;

    /// <summary>
    ///     Display name for the space.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     Stable slug used in routes, APIs, and integrations.
    /// </summary>
    public required string Slug { get; set; }

    /// <summary>
    ///     Description of the type of question and context expected in the space.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    ///     Language used for curation, search, and rendering in the space.
    /// </summary>
    public required string Language { get; set; }

    /// <summary>
    ///     Operating mode for controlled publication, moderated collaboration, or public validation.
    /// </summary>
    public required SpaceKind Kind { get; set; }

    /// <summary>
    ///     Visibility scope for the space.
    /// </summary>
    public required VisibilityScope Visibility { get; set; }

    /// <summary>
    ///     Indicates whether the space accepts new questions.
    /// </summary>
    public required bool AcceptsQuestions { get; set; }

    /// <summary>
    ///     Indicates whether the space accepts answers.
    /// </summary>
    public required bool AcceptsAnswers { get; set; }

    /// <summary>
    ///     Timestamp when the space was published for consumption.
    /// </summary>
    public DateTime? PublishedAtUtc { get; set; }

    /// <summary>
    ///     Questions that belong to the space.
    /// </summary>
    public ICollection<Question> Questions { get; set; } = [];

    /// <summary>
    ///     Relationships between the space and the tags that classify it.
    /// </summary>
    public ICollection<SpaceTag> Tags { get; set; } = [];

    /// <summary>
    ///     Relationships between the space and the curated sources available to it.
    /// </summary>
    public ICollection<SpaceSource> Sources { get; set; } = [];

    /// <summary>
    ///     Tenant that owns the space.
    /// </summary>
    public required Guid TenantId { get; set; }
}
