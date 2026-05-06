namespace Querify.Models.QnA.Enums;

/// <summary>
/// Classifies the kind of source material linked to the domain.
/// </summary>
public enum SourceKind
{
    /// <summary>
    /// A curated article or knowledge document.
    /// </summary>
    Article = 1,

    /// <summary>
    /// A standard web page used as reference or origin.
    /// </summary>
    WebPage = 6,

    /// <summary>
    /// A PDF document such as manuals, policy docs, or exported guides.
    /// </summary>
    Pdf = 11,

    /// <summary>
    /// A video source where the answer may depend on timestamps or transcripts.
    /// </summary>
    Video = 16,

    /// <summary>
    /// A source repository or code-hosting artifact.
    /// Useful for developer-facing Q&amp;A.
    /// </summary>
    Repository = 21,

    /// <summary>
    /// A product note such as release note, change log, or feature note.
    /// </summary>
    ProductNote = 26,

    /// <summary>
    /// An internal-only note or operational record.
    /// </summary>
    InternalNote = 31,

    /// <summary>
    /// A proposal, decision record, or governance discussion artifact.
    /// </summary>
    GovernanceRecord = 36,

    /// <summary>
    /// An audit record, signed payload, or external verification anchor.
    /// </summary>
    AuditRecord = 41,

    /// <summary>
    /// Fallback for uncommon or still-uncategorized source artifacts.
    /// </summary>
    Other = 46
}
