using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Guards;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Context-specific link from an answer to a source artifact.
/// </summary>
public sealed class AnswerSourceLink : BaseEntity, IMustHaveTenant
{
    public const int MaxLabelLength = 200;
    public const int MaxScopeLength = 1000;
    public const int MaxExcerptLength = 4000;

    private AnswerSourceLink()
    {
    }

    public AnswerSourceLink(
        Answer answer,
        KnowledgeSource source,
        SourceRole role = SourceRole.Evidence,
        string? label = null,
        string? scope = null,
        string? excerpt = null,
        int order = 0,
        int confidenceScore = 0,
        bool isPrimary = false,
        string? createdBy = null)
    {
        ArgumentNullException.ThrowIfNull(answer);
        ArgumentNullException.ThrowIfNull(source);

        Id = Guid.NewGuid();
        TenantId = DomainGuards.TenantIdOf(answer, nameof(answer));
        DomainGuards.InitializeAudit(this, createdBy);
        DomainGuards.EnsureSameTenant(answer, source, "answer to knowledge source");

        AnswerId = answer.Id;
        Answer = answer;
        SourceId = source.Id;
        Source = source;
        Role = role;
        Label = DomainGuards.Optional(label, MaxLabelLength, nameof(label));
        Scope = DomainGuards.Optional(scope, MaxScopeLength, nameof(scope));
        Excerpt = DomainGuards.Optional(excerpt, MaxExcerptLength, nameof(excerpt));
        Order = DomainGuards.NonNegative(order, nameof(order));
        ConfidenceScore = DomainGuards.Range(confidenceScore, 0, 100, nameof(confidenceScore));
        IsPrimary = isPrimary;

        EnsureCompatibleWithVisibility(answer.Visibility);
    }

    /// <summary>
    /// Tenant boundary copied from the owning answer/source pair.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Owning answer identifier.
    /// </summary>
    public Guid AnswerId { get; private set; }

    /// <summary>
    /// Owning answer navigation.
    /// </summary>
    public Answer Answer { get; private set; } = null!;

    /// <summary>
    /// Linked source identifier.
    /// </summary>
    public Guid SourceId { get; private set; }

    /// <summary>
    /// Linked source navigation.
    /// </summary>
    public KnowledgeSource Source { get; private set; } = null!;

    /// <summary>
    /// Explains why the source is attached to the answer.
    /// </summary>
    public SourceRole Role { get; private set; } = SourceRole.Evidence;

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
