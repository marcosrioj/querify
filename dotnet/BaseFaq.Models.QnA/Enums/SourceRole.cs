namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Explains why a source is attached to a question or answer.
/// </summary>
public enum SourceRole
{
    /// <summary>
    /// The source explains where the question came from.
    /// </summary>
    Origin = 1,

    /// <summary>
    /// The source adds background or interpretation for the question or answer.
    /// </summary>
    Context = 2,

    /// <summary>
    /// The source directly supports the question or answer and should influence confidence.
    /// </summary>
    Evidence = 3,

    /// <summary>
    /// The source is a user-facing reference for the current question or answer.
    /// </summary>
    Reference = 4
}
