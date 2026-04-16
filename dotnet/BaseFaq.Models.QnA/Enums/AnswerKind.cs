namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Explains where an answer came from and how it should be interpreted.
/// </summary>
public enum AnswerKind
{
    /// <summary>
    /// An official answer owned by the brand, product, or support operation.
    /// This is the strongest governance mode.
    /// </summary>
    Official = 1,

    /// <summary>
    /// An answer provided by a community participant.
    /// It may still become accepted, but it is not official by default.
    /// </summary>
    Community = 2,

    /// <summary>
    /// An AI-generated draft that should not be treated as trusted until reviewed.
    /// It is useful for acceleration, not direct publication.
    /// </summary>
    AiDraft = 3,

    /// <summary>
    /// An answer created or edited by a human with AI assistance.
    /// This normally carries more trust than a raw draft but still benefits from review.
    /// </summary>
    AiAssisted = 4,

    /// <summary>
    /// An answer imported from another system or knowledge base.
    /// It may need normalization or revalidation in the local domain.
    /// </summary>
    Imported = 5
}
