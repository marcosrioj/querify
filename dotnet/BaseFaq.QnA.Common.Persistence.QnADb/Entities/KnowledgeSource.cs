using System.ComponentModel.DataAnnotations.Schema;
using BaseFaq.Common.EntityFramework.Core.Abstractions;
using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.QnA.Enums;
using BaseFaq.QnA.Common.Persistence.QnADb.Guards;

namespace BaseFaq.QnA.Common.Persistence.QnADb.Entities;

/// <summary>
/// Durable record of a source artifact used as origin, context, evidence, or citation.
/// </summary>
public sealed class KnowledgeSource : BaseEntity, IMustHaveTenant
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

    private KnowledgeSource()
    {
    }

    public KnowledgeSource(Guid tenantId, SourceKind kind, string locator, string? createdBy = null)
    {
        Id = Guid.NewGuid();
        TenantId = DomainGuards.AgainstEmpty(tenantId, nameof(tenantId));
        DomainGuards.InitializeAudit(this, createdBy);
        Kind = kind;
        Locator = DomainGuards.Required(locator, MaxLocatorLength, nameof(locator));
    }

    /// <summary>
    /// Tenant boundary that owns the source artifact and its reuse permissions.
    /// </summary>
    public Guid TenantId { get; private set; }

    /// <summary>
    /// Classifies the type of source artifact.
    /// </summary>
    public SourceKind Kind { get; private set; } = SourceKind.Other;

    /// <summary>
    /// Stable pointer to the source artifact, such as URL, ticket id, message id, file path, or document id.
    /// </summary>
    public string Locator { get; private set; } = null!;

    /// <summary>
    /// Human-readable label for the artifact.
    /// </summary>
    public string? Label { get; private set; }

    /// <summary>
    /// Optional scope description such as page range, timestamp range, or subsystem.
    /// </summary>
    public string? Scope { get; private set; }

    /// <summary>
    /// Upstream provider or system, such as Zendesk, GitHub, Slack, Intercom, or YouTube.
    /// </summary>
    public string? SystemName { get; private set; }

    /// <summary>
    /// External identifier from the upstream system.
    /// </summary>
    public string? ExternalId { get; private set; }

    /// <summary>
    /// Language of the source artifact.
    /// </summary>
    public string? Language { get; private set; }

    /// <summary>
    /// Media type or MIME-like hint for the artifact.
    /// </summary>
    public string? MediaType { get; private set; }

    /// <summary>
    /// Optional checksum used to detect source drift.
    /// </summary>
    public string? Checksum { get; private set; }

    /// <summary>
    /// Flexible metadata payload used by connectors and ingestion workflows.
    /// </summary>
    public string? MetadataJson { get; private set; }

    /// <summary>
    /// Audience exposure of the source itself.
    /// </summary>
    public VisibilityScope Visibility { get; private set; } = VisibilityScope.Internal;

    /// <summary>
    /// Indicates whether this source may be cited on public surfaces.
    /// </summary>
    public bool AllowsPublicCitation { get; private set; }

    /// <summary>
    /// Indicates whether public-facing excerpts may be reused from this source.
    /// </summary>
    public bool AllowsPublicExcerpt { get; private set; }

    /// <summary>
    /// Indicates whether the source is currently treated as authoritative.
    /// </summary>
    public bool IsAuthoritative { get; private set; }

    /// <summary>
    /// Timestamp when the artifact was captured into the knowledge system.
    /// </summary>
    public DateTime? CapturedAtUtc { get; private set; }

    /// <summary>
    /// Timestamp of the last explicit verification pass.
    /// </summary>
    public DateTime? LastVerifiedAtUtc { get; private set; }

    /// <summary>
    /// Many-to-many persistence links from question spaces to this source.
    /// </summary>
    public ICollection<QuestionSpaceSource> QuestionSpaceSources { get; private set; } = [];

    /// <summary>
    /// Question links that use this source for origin or context.
    /// </summary>
    public ICollection<QuestionSourceLink> Questions { get; private set; } = [];

    /// <summary>
    /// Answer links that use this source for evidence or citation.
    /// </summary>
    public ICollection<AnswerSourceLink> Answers { get; private set; } = [];

    /// <summary>
    /// Spaces that curate this source at collection level.
    /// </summary>
    [NotMapped]
    public IReadOnlyCollection<QuestionSpace> Spaces => QuestionSpaceSources.Select(link => link.QuestionSpace).ToList();

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void UpdateMetadata(
        string locator,
        string? label = null,
        string? scope = null,
        string? systemName = null,
        string? externalId = null,
        string? language = null,
        string? mediaType = null,
        string? checksum = null,
        string? metadataJson = null,
        DateTime? capturedAtUtc = null,
        string? updatedBy = null,
        DateTime? updatedAtUtc = null)
    {
        Locator = DomainGuards.Required(locator, MaxLocatorLength, nameof(locator));
        Label = DomainGuards.Optional(label, MaxLabelLength, nameof(label));
        Scope = DomainGuards.Optional(scope, MaxScopeLength, nameof(scope));
        SystemName = DomainGuards.Optional(systemName, MaxSystemNameLength, nameof(systemName));
        ExternalId = DomainGuards.Optional(externalId, MaxExternalIdLength, nameof(externalId));
        Language = DomainGuards.Optional(language, MaxLanguageLength, nameof(language));
        MediaType = DomainGuards.Optional(mediaType, MaxMediaTypeLength, nameof(mediaType));
        Checksum = DomainGuards.Optional(checksum, MaxChecksumLength, nameof(checksum));
        MetadataJson = DomainGuards.Json(metadataJson, MaxMetadataLength, nameof(metadataJson));
        CapturedAtUtc = capturedAtUtc is null ? CapturedAtUtc : DomainGuards.Utc(capturedAtUtc.Value, nameof(capturedAtUtc));
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void MarkVerified(bool isAuthoritative, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        var verifiedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));

        IsAuthoritative = isAuthoritative;
        LastVerifiedAtUtc = verifiedAtUtc;
        DomainGuards.Touch(this, updatedBy, verifiedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void ConfigureExposure(
        VisibilityScope visibility,
        bool allowsPublicCitation = false,
        bool allowsPublicExcerpt = false,
        string? updatedBy = null,
        DateTime? updatedAtUtc = null)
    {
        if (visibility.IsPubliclyVisible())
        {
            DomainGuards.Ensure(
                Kind != SourceKind.InternalNote,
                "Internal notes cannot be exposed on public surfaces.");
            DomainGuards.Ensure(
                LastVerifiedAtUtc is not null,
                "Sources must be verified before public exposure is allowed.");
        }
        else
        {
            allowsPublicCitation = false;
            allowsPublicExcerpt = false;
        }

        Visibility = visibility;
        AllowsPublicCitation = allowsPublicCitation;
        AllowsPublicExcerpt = allowsPublicExcerpt;
        DomainGuards.Touch(this, updatedBy, updatedAtUtc);
    }

    // TODO(qna-handlers): Migrate this rich-model method to the write handlers.
    public void EnsureCompatibleWithVisibility(VisibilityScope targetVisibility, SourceRole role, bool includesExcerpt)
    {
        if (!targetVisibility.IsPubliclyVisible())
        {
            return;
        }

        if (role is SourceRole.Citation or SourceRole.CanonicalReference)
        {
            DomainGuards.Ensure(
                Visibility.IsPubliclyVisible() && AllowsPublicCitation,
                "Public citations require a publicly visible source that explicitly allows citation.");
        }

        if (includesExcerpt)
        {
            DomainGuards.Ensure(
                Visibility.IsPubliclyVisible() && AllowsPublicExcerpt,
                "Public excerpts require a publicly visible source that explicitly allows excerpt reuse.");
        }
    }

    internal void AttachToQuestion(QuestionSourceLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        DomainGuards.EnsureSameTenant(this, link, "knowledge source to question source link");

        if (Questions.Any(existing => existing.Id == link.Id))
        {
            return;
        }

        Questions.Add(link);
    }

    internal void AttachToAnswer(AnswerSourceLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        DomainGuards.EnsureSameTenant(this, link, "knowledge source to answer source link");

        if (Answers.Any(existing => existing.Id == link.Id))
        {
            return;
        }

        Answers.Add(link);
    }
}
