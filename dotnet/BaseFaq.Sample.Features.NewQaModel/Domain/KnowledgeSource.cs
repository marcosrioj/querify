using BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class KnowledgeSource : DomainEntity
{
    public const int MaxLocatorLength = 1000;
    public const int MaxLabelLength = 200;
    public const int MaxScopeLength = 1000;
    public const int MaxSystemNameLength = 100;
    public const int MaxExternalIdLength = 250;
    public const int MaxLanguageLength = 50;
    public const int MaxMediaTypeLength = 100;
    public const int MaxChecksumLength = 128;
    public const int MaxMetadataLength = 8000;

    public SourceKind Kind { get; set; }

    /// <summary>
    /// Stable pointer to the source artifact, such as URL, ticket id, message id, file path, or document id.
    /// </summary>
    public required string Locator { get; set; }

    public string? Label { get; set; }
    public string? Scope { get; set; }

    /// <summary>
    /// Upstream provider or system, such as Zendesk, GitHub, Slack, Intercom, or YouTube.
    /// </summary>
    public string? SystemName { get; set; }

    public string? ExternalId { get; set; }
    public string? Language { get; set; }
    public string? MediaType { get; set; }
    public string? Checksum { get; set; }
    public string? MetadataJson { get; set; }

    public bool IsAuthoritative { get; set; }
    public DateTime? CapturedAtUtc { get; set; }
    public DateTime? LastVerifiedAtUtc { get; set; }

    public ICollection<QuestionSpace> Spaces { get; set; } = [];
    public ICollection<QuestionSourceLink> Questions { get; set; } = [];
    public ICollection<AnswerSourceLink> Answers { get; set; } = [];
}
