namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class Topic : DomainEntity
{
    public const int MaxNameLength = 100;
    public const int MaxCategoryLength = 100;
    public const int MaxDescriptionLength = 500;

    /// <summary>
    /// Normalized topic label such as "billing", "api", "shipping", or "troubleshooting".
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional grouping such as product, journey, plan, version, or integration.
    /// </summary>
    public string? Category { get; set; }

    public string? Description { get; set; }

    public ICollection<QuestionSpace> Spaces { get; set; } = [];
    public ICollection<Question> Questions { get; set; } = [];
}
