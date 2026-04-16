using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Represents a Q&amp;A space where questions live, including exposure,
/// moderation, and curation rules.
/// </summary>
public class Space : BaseEntity, IMustHaveTenant
{
    public const int MaxNameLength = 200;
    public const int MaxKeyLength = 160;
    public const int MaxSummaryLength = 2000;
    public const int MaxLanguageLength = 50;
    public const int MaxProductScopeLength = 200;
    public const int MaxJourneyScopeLength = 200;

    /// <summary>
    /// Display name for the space.
    /// </summary>
    public required string Name { get; set; } = null!;

    /// <summary>
    /// Stable key used in routes, APIs, and integrations.
    /// </summary>
    public required string Key { get; set; } = null!;

    /// <summary>
    /// Description of the type of question and context expected in the space.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Default language used for curation, search, and rendering.
    /// </summary>
    public required string DefaultLanguage { get; set; } = null!;

    /// <summary>
    /// Functional kind of the space, such as curated knowledge or community.
    /// </summary>
    public required SpaceKind Kind { get; set; } = SpaceKind.CuratedKnowledge;

    /// <summary>
    /// Visibility scope for the space.
    /// </summary>
    public required VisibilityScope Visibility { get; set; } = VisibilityScope.Internal;

    /// <summary>
    /// Moderation policy applied to the space workflow.
    /// </summary>
    public required ModerationPolicy ModerationPolicy { get; set; } = ModerationPolicy.PreModeration;

    /// <summary>
    /// Controls markup behavior for search and indexable surfaces.
    /// </summary>
    public required SearchMarkupMode SearchMarkupMode { get; set; } = SearchMarkupMode.Off;

    /// <summary>
    /// Optional product scope covered by the space.
    /// </summary>
    public string? ProductScope { get; set; }

    /// <summary>
    /// Optional journey scope covered by the space.
    /// </summary>
    public string? JourneyScope { get; set; }

    /// <summary>
    /// Indicates whether the space accepts new questions.
    /// </summary>
    public required bool AcceptsQuestions { get; set; }

    /// <summary>
    /// Indicates whether the space accepts answers.
    /// </summary>
    public required bool AcceptsAnswers { get; set; }

    /// <summary>
    /// Defines whether new questions require review before moving through the workflow.
    /// </summary>
    public required bool RequiresQuestionReview { get; set; } = true;

    /// <summary>
    /// Defines whether new answers require review before exposure.
    /// </summary>
    public required bool RequiresAnswerReview { get; set; } = true;

    /// <summary>
    /// Timestamp when the space was published for consumption.
    /// </summary>
    public DateTime? PublishedAtUtc { get; set; }

    /// <summary>
    /// Timestamp of the last operational or editorial validation of the space.
    /// </summary>
    public DateTime? LastValidatedAtUtc { get; set; }

    /// <summary>
    /// Questions that belong to the space.
    /// </summary>
    public ICollection<Question> Questions { get; set; } = [];

    /// <summary>
    /// Relationships between the space and the tags that classify it.
    /// </summary>
    public ICollection<SpaceTag> Tags { get; set; } = [];

    /// <summary>
    /// Relationships between the space and the curated sources available to it.
    /// </summary>
    public ICollection<SpaceSource> Sources { get; set; } = [];

    /// <summary>
    /// Tenant that owns the space.
    /// </summary>
    public required Guid TenantId { get; set; }
}
