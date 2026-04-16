namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class Tag : DomainEntity
{
    public const int MaxNameLength = 100;

    private readonly List<Space> spaces = [];
    private readonly List<Question> questions = [];

    private Tag()
    {
    }

    public Tag(Guid tenantId, string name, string? createdBy = null)
        : base(tenantId, createdBy)
    {
        Name = DomainGuards.Required(name, MaxNameLength, nameof(name));
    }

    /// <summary>
    /// Normalized tag label such as "billing", "api", "shipping", or "troubleshooting".
    /// </summary>
    public string Name { get; private set; } = null!;

    public IReadOnlyCollection<Space> Spaces => spaces;
    public IReadOnlyCollection<Question> Questions => questions;

    public void UpdateMetadata(string name, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        Name = DomainGuards.Required(name, MaxNameLength, nameof(name));
        Touch(updatedBy, updatedAtUtc);
    }

    internal void AttachToSpace(Space space)
    {
        ArgumentNullException.ThrowIfNull(space);
        EnsureSameTenant(space, "tag to space");

        if (spaces.Any(existing => existing.Id == space.Id))
        {
            return;
        }

        spaces.Add(space);
    }

    internal void AttachToQuestion(Question question)
    {
        ArgumentNullException.ThrowIfNull(question);
        EnsureSameTenant(question, "tag to question");

        if (questions.Any(existing => existing.Id == question.Id))
        {
            return;
        }

        questions.Add(question);
    }
}
