using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
///     Represents a source used as an origin, evidence, or reference
///     for spaces, questions, and answers.
/// </summary>
public class Source : BaseEntity, IMustHaveTenant
{
    public const int MaxLocatorLength = 1000;
    public const int MaxLabelLength = 200;
    public const int MaxContextNoteLength = 2000;
    public const int MaxExternalIdLength = 250;
    public const int MaxLanguageLength = 50;
    public const int MaxMediaTypeLength = 100;
    public const int MaxChecksumLength = 128;
    public const int MaxMetadataLength = 8000;

    /// <summary>
    ///     Source kind, such as document, ticket, chat, page, or internal note.
    /// </summary>
    public required SourceKind Kind { get; set; }

    /// <summary>
    ///     Stable pointer to the source, such as a URL, external id, or path.
    /// </summary>
    public required string Locator { get; set; }

    /// <summary>
    ///     Human-readable label for the source.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    ///     Human-readable context that explains how the source should be used.
    /// </summary>
    public string? ContextNote { get; set; }

    /// <summary>
    ///     External identifier of the source in the upstream system.
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    ///     Language of the source content.
    /// </summary>
    public required string Language { get; set; }

    /// <summary>
    ///     Media type or content format.
    /// </summary>
    public string? MediaType { get; set; }

    /// <summary>
    ///     Hash or signature used to detect content changes.
    /// </summary>
    public required string Checksum { get; set; }

    /// <summary>
    ///     Serialized source metadata.
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    ///     Allowed visibility scope for the source.
    /// </summary>
    public required VisibilityScope Visibility { get; set; }

    /// <summary>
    ///     Timestamp of the last trust or freshness verification.
    /// </summary>
    public DateTime? LastVerifiedAtUtc { get; set; }

    /// <summary>
    ///     Spaces that curate or expose this source.
    /// </summary>
    public ICollection<SpaceSource> Spaces { get; set; } = [];

    /// <summary>
    ///     Links between the source and questions.
    /// </summary>
    public ICollection<QuestionSourceLink> Questions { get; set; } = [];

    /// <summary>
    ///     Links between the source and answers.
    /// </summary>
    public ICollection<AnswerSourceLink> Answers { get; set; } = [];

    /// <summary>
    ///     Tenant that owns the source.
    /// </summary>
    public required Guid TenantId { get; set; }
}
