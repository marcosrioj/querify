namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Explains how a question entered the domain.
/// </summary>
public enum QuestionKind
{
    /// <summary>
    /// A curated, intentionally authored question.
    /// Usually created by product, support, marketing, or documentation teams.
    /// </summary>
    Curated = 1,

    /// <summary>
    /// A question created by a community participant.
    /// Useful for open Q&amp;A and support-community flows.
    /// </summary>
    Community = 2,

    /// <summary>
    /// A question imported from an external system such as a ticket queue or community platform.
    /// </summary>
    Imported = 3
}
