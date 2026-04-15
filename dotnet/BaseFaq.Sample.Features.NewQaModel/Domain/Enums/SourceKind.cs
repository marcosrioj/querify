namespace BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

/// <summary>
///     Classifies the kind of source material linked to the domain.
/// </summary>
public enum SourceKind
{
    /// <summary>
    ///     A curated article or knowledge document.
    /// </summary>
    Article = 1,

    /// <summary>
    ///     A standard web page used as reference or origin.
    /// </summary>
    WebPage = 2,

    /// <summary>
    ///     A PDF document such as manuals, policy docs, or exported guides.
    /// </summary>
    Pdf = 3,

    /// <summary>
    ///     A video source where the answer may depend on timestamps or transcripts.
    /// </summary>
    Video = 4,

    /// <summary>
    ///     A source repository or code-hosting artifact.
    ///     Useful for developer-facing Q&A.
    /// </summary>
    Repository = 5,

    /// <summary>
    ///     A support ticket or resolved case.
    /// </summary>
    Ticket = 6,

    /// <summary>
    ///     A community thread or discussion topic.
    /// </summary>
    CommunityThread = 7,

    /// <summary>
    ///     A social-media comment or reply chain.
    /// </summary>
    SocialComment = 8,

    /// <summary>
    ///     A chat or messaging transcript.
    /// </summary>
    ChatTranscript = 9,

    /// <summary>
    ///     A product note such as release note, change log, or feature note.
    /// </summary>
    ProductNote = 10,

    /// <summary>
    ///     An internal-only note or operational record.
    /// </summary>
    InternalNote = 11,

    /// <summary>
    ///     Fallback for uncommon or still-uncategorized source artifacts.
    /// </summary>
    Other = 99
}