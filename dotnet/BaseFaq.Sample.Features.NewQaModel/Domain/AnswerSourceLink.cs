using BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class AnswerSourceLink : DomainEntity
{
    public const int MaxLabelLength = 200;
    public const int MaxScopeLength = 1000;
    public const int MaxExcerptLength = 4000;

    private AnswerSourceLink()
    {
    }

    public AnswerSourceLink(
        Answer answer,
        Source source,
        SourceRole role = SourceRole.Evidence,
        string? label = null,
        string? scope = null,
        string? excerpt = null,
        int order = 0,
        int confidenceScore = 0,
        bool isPrimary = false,
        string? createdBy = null)
        : base(DomainGuards.TenantIdOf(answer, nameof(answer)), createdBy)
    {
        ArgumentNullException.ThrowIfNull(answer);
        ArgumentNullException.ThrowIfNull(source);

        EnsureSameTenant(answer, source, "answer to source");

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

    public Guid AnswerId { get; private set; }
    public Answer Answer { get; private set; } = null!;

    public Guid SourceId { get; private set; }
    public Source Source { get; private set; } = null!;

    /// <summary>
    /// Explains why the source is attached to the answer.
    /// </summary>
    public SourceRole Role { get; private set; } = SourceRole.Evidence;

    public string? Label { get; private set; }
    public string? Scope { get; private set; }
    public string? Excerpt { get; private set; }

    public int Order { get; private set; }
    public int ConfidenceScore { get; private set; }
    public bool IsPrimary { get; private set; }

    internal void EnsureCompatibleWithVisibility(VisibilityScope targetVisibility)
    {
        Source.EnsureCompatibleWithVisibility(targetVisibility, Role, Excerpt is not null);
    }
}
