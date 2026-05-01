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
    Community = 6,

    /// <summary>
    /// An answer imported from another system or knowledge base.
    /// It may need normalization or revalidation in the local domain.
    /// </summary>
    Imported = 11
}
