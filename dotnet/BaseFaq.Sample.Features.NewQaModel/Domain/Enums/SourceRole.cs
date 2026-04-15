namespace BaseFaq.Sample.Features.NewQaModel.Domain.Enums;

/// <summary>
///     Explains why a source is attached to a question or answer.
/// </summary>
public enum SourceRole
{
    /// <summary>
    ///     The source explains where the question came from.
    /// </summary>
    QuestionOrigin = 1,

    /// <summary>
    ///     The source adds background or interpretation, but is not the strongest proof.
    /// </summary>
    SupportingContext = 2,

    /// <summary>
    ///     The source directly supports the answer and should influence confidence.
    /// </summary>
    Evidence = 3,

    /// <summary>
    ///     The source is intended to be shown or cited explicitly in user-facing trust surfaces.
    /// </summary>
    Citation = 4,

    /// <summary>
    ///     The source acts as the canonical reference that should anchor the current answer.
    /// </summary>
    CanonicalReference = 5
}