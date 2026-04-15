namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class Topic : DomainEntity
{
    public const int MaxNameLength = 100;
    public const int MaxCategoryLength = 100;
    public const int MaxDescriptionLength = 500;

    private readonly List<QuestionSpace> spaces = [];
    private readonly List<Question> questions = [];

    private Topic()
    {
    }

    public Topic(Guid tenantId, string name, string? category = null, string? description = null, string? createdBy = null)
        : base(tenantId, createdBy)
    {
        Name = DomainGuards.Required(name, MaxNameLength, nameof(name));
        Category = DomainGuards.Optional(category, MaxCategoryLength, nameof(category));
        Description = DomainGuards.Optional(description, MaxDescriptionLength, nameof(description));
    }

    /// <summary>
    /// Normalized topic label such as "billing", "api", "shipping", or "troubleshooting".
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Optional grouping such as product, journey, plan, version, or integration.
    /// </summary>
    public string? Category { get; private set; }

    public string? Description { get; private set; }

    public IReadOnlyCollection<QuestionSpace> Spaces => spaces;
    public IReadOnlyCollection<Question> Questions => questions;

    public void UpdateMetadata(string name, string? category = null, string? description = null, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        Name = DomainGuards.Required(name, MaxNameLength, nameof(name));
        Category = DomainGuards.Optional(category, MaxCategoryLength, nameof(category));
        Description = DomainGuards.Optional(description, MaxDescriptionLength, nameof(description));
        Touch(updatedBy, updatedAtUtc);
    }

    internal void AttachToSpace(QuestionSpace space)
    {
        ArgumentNullException.ThrowIfNull(space);
        EnsureSameTenant(space, "topic to question space");

        if (spaces.Any(existing => existing.Id == space.Id))
        {
            return;
        }

        spaces.Add(space);
    }

    internal void AttachToQuestion(Question question)
    {
        ArgumentNullException.ThrowIfNull(question);
        EnsureSameTenant(question, "topic to question");

        if (questions.Any(existing => existing.Id == question.Id))
        {
            return;
        }

        questions.Add(question);
    }
}
