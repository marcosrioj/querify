namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Defines the lifecycle of an answer candidate.
/// </summary>
public enum AnswerStatus
{
    /// <summary>
    /// The answer is still being prepared.
    /// It is not available to end users.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The answer is ready for operational use and can be exposed when visibility allows it.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The answer is no longer active in the knowledge surface.
    /// Historical data may still be preserved.
    /// </summary>
    Archived = 2
}
