using System.ComponentModel.DataAnnotations.Schema;
using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.QnA.Common.Persistence.QnADb.Guards;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Lightweight taxonomy label reused across question spaces and question threads.
/// </summary>
public sealed class Topic : BaseEntity, IMustHaveTenant
{
    public const int MaxNameLength = 100;
    public const int MaxCategoryLength = 100;
    public const int MaxDescriptionLength = 500;

    private Topic()
    {
    }

    public Topic(Guid tenantId, string name, string? category = null, string? description = null, string? createdBy = null)
    {
        Id = Guid.NewGuid();
        TenantId = DomainGuards.AgainstEmpty(tenantId, nameof(tenantId));
        DomainGuards.InitializeAudit(this, createdBy);
        Name = DomainGuards.Required(name, MaxNameLength, nameof(name));
        Category = DomainGuards.Optional(category, MaxCategoryLength, nameof(category));
        Description = DomainGuards.Optional(description, MaxDescriptionLength, nameof(description));
    }

    /// <summary>
    /// Tenant boundary that owns the taxonomy label.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Normalized topic label such as billing, api, shipping, or troubleshooting.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Optional grouping such as product, journey, plan, version, or integration.
    /// </summary>
    public string? Category { get; private set; }

    /// <summary>
    /// Human-readable explanation of what the topic covers.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Persistence links that connect the topic to question spaces.
    /// </summary>
    public ICollection<QuestionSpaceTopic> QuestionSpaceTopics { get; private set; } = [];

    /// <summary>
    /// Persistence links that connect the topic to questions.
    /// </summary>
    public ICollection<QuestionTopic> QuestionTopics { get; private set; } = [];

    /// <summary>
    /// Spaces currently classified by the topic.
    /// </summary>
    [NotMapped]
    public IReadOnlyCollection<QuestionSpace> Spaces => QuestionSpaceTopics.Select(link => link.QuestionSpace).ToList();

    /// <summary>
    /// Questions currently classified by the topic.
    /// </summary>
    [NotMapped]
    public IReadOnlyCollection<Question> Questions => QuestionTopics.Select(link => link.Question).ToList();

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void UpdateMetadata(string name, string? category = null, string? description = null, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        Name = DomainGuards.Required(name, MaxNameLength, nameof(name));
        Category = DomainGuards.Optional(category, MaxCategoryLength, nameof(category));
        Description = DomainGuards.Optional(description, MaxDescriptionLength, nameof(description));
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }
}
