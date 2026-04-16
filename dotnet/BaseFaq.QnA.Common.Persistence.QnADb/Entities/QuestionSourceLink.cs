using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Guards;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Context-specific link from a question to a source artifact.
/// </summary>
public sealed class QuestionSourceLink : BaseEntity, IMustHaveTenant
{
    public const int MaxLabelLength = 200;
    public const int MaxScopeLength = 1000;
    public const int MaxExcerptLength = 4000;

    private QuestionSourceLink()
    {
    }

    public QuestionSourceLink(
        Question question,
        KnowledgeSource source,
        SourceRole role = SourceRole.QuestionOrigin,
        string? label = null,
        string? scope = null,
        string? excerpt = null,
        int order = 0,
        int confidenceScore = 0,
        bool isPrimary = false,
        string? createdBy = null)
    {
        ArgumentNullException.ThrowIfNull(question);
        ArgumentNullException.ThrowIfNull(source);

        Id = Guid.NewGuid();
        TenantId = DomainGuards.TenantIdOf(question, nameof(question));
        DomainGuards.InitializeAudit(this, createdBy);
        DomainGuards.EnsureSameTenant(question, source, "question to knowledge source");

        QuestionId = question.Id;
        Question = question;
        SourceId = source.Id;
        Source = source;
        Role = role;
        Label = DomainGuards.Optional(label, MaxLabelLength, nameof(label));
        Scope = DomainGuards.Optional(scope, MaxScopeLength, nameof(scope));
        Excerpt = DomainGuards.Optional(excerpt, MaxExcerptLength, nameof(excerpt));
        Order = DomainGuards.NonNegative(order, nameof(order));
        ConfidenceScore = DomainGuards.Range(confidenceScore, 0, 100, nameof(confidenceScore));
        IsPrimary = isPrimary;

        EnsureCompatibleWithVisibility(question.Visibility);
    }

    /// <summary>
    /// Tenant boundary copied from the owning question/source pair.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Owning question identifier.
    /// </summary>
    public Guid QuestionId { get; private set; }

    /// <summary>
    /// Owning question navigation.
    /// </summary>
    public Question Question { get; private set; } = null!;

    /// <summary>
    /// Linked source identifier.
    /// </summary>
    public Guid SourceId { get; private set; }

    /// <summary>
    /// Linked source navigation.
    /// </summary>
    public KnowledgeSource Source { get; private set; } = null!;

    /// <summary>
    /// Explains why the source is attached to the question.
    /// </summary>
    public SourceRole Role { get; private set; } = SourceRole.QuestionOrigin;

    /// <summary>
    /// Optional display label for this specific link context.
    /// </summary>
    public string? Label { get; private set; }

    /// <summary>
    /// Optional scope detail such as timestamp range, section, or document region.
    /// </summary>
    public string? Scope { get; private set; }

    /// <summary>
    /// Optional excerpt reused from the linked source.
    /// </summary>
    public string? Excerpt { get; private set; }

    /// <summary>
    /// Display or processing order among multiple links.
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Link-specific confidence score.
    /// </summary>
    public int ConfidenceScore { get; private set; }

    /// <summary>
    /// Indicates whether this is the primary source link for the current purpose.
    /// </summary>
    public bool IsPrimary { get; private set; }

    internal void EnsureCompatibleWithVisibility(VisibilityScope targetVisibility)
    {
        Source.EnsureCompatibleWithVisibility(targetVisibility, Role, Excerpt is not null);
    }
}
