using BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

namespace BaseFaq.Sample.Features.NewQaModel.Domain;

public sealed class Source : DomainEntity
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

    private readonly List<Space> spaces = [];
    private readonly List<QuestionSourceLink> questions = [];
    private readonly List<AnswerSourceLink> answers = [];

    private Source()
    {
    }

    public Source(Guid tenantId, SourceKind kind, string locator, string? createdBy = null)
        : base(tenantId, createdBy)
    {
        Kind = kind;
        Locator = DomainGuards.Required(locator, MaxLocatorLength, nameof(locator));
    }

    public SourceKind Kind { get; private set; }

    /// <summary>
    /// Stable pointer to the source artifact, such as URL, ticket id, message id, file path, or document id.
    /// </summary>
    public string Locator { get; private set; } = null!;

    public string? Label { get; private set; }
    public string? Scope { get; private set; }

    /// <summary>
    /// Upstream provider or system, such as Zendesk, GitHub, Slack, Intercom, or YouTube.
    /// </summary>
    public string? SystemName { get; private set; }

    public string? ExternalId { get; private set; }
    public string? Language { get; private set; }
    public string? MediaType { get; private set; }
    public string? Checksum { get; private set; }
    public string? MetadataJson { get; private set; }

    public VisibilityScope Visibility { get; private set; } = VisibilityScope.Internal;
    public bool AllowsPublicCitation { get; private set; }
    public bool AllowsPublicExcerpt { get; private set; }
    public bool IsAuthoritative { get; private set; }
    public DateTime? CapturedAtUtc { get; private set; }
    public DateTime? LastVerifiedAtUtc { get; private set; }

    public IReadOnlyCollection<Space> Spaces => spaces;
    public IReadOnlyCollection<QuestionSourceLink> Questions => questions;
    public IReadOnlyCollection<AnswerSourceLink> Answers => answers;

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
        Touch(updatedBy, updatedAtUtc);
    }

    public void MarkVerified(bool isAuthoritative, string? updatedBy = null, DateTime? updatedAtUtc = null)
    {
        var verifiedAtUtc = DomainGuards.Utc(updatedAtUtc ?? DateTime.UtcNow, nameof(updatedAtUtc));

        IsAuthoritative = isAuthoritative;
        LastVerifiedAtUtc = verifiedAtUtc;
        Touch(updatedBy, verifiedAtUtc);
    }

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
        Touch(updatedBy, updatedAtUtc);
    }

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

    internal void AttachToSpace(Space space)
    {
        ArgumentNullException.ThrowIfNull(space);
        EnsureSameTenant(space, "source to space");

        if (spaces.Any(existing => existing.Id == space.Id))
        {
            return;
        }

        spaces.Add(space);
    }

    internal void AttachToQuestion(QuestionSourceLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        EnsureSameTenant(link, "source to question source link");

        if (questions.Any(existing => existing.Id == link.Id))
        {
            return;
        }

        questions.Add(link);
    }

    internal void AttachToAnswer(AnswerSourceLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        EnsureSameTenant(link, "source to answer source link");

        if (answers.Any(existing => existing.Id == link.Id))
        {
            return;
        }

        answers.Add(link);
    }
}
