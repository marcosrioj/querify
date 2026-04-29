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
    Active = 2,

    /// <summary>
    /// The answer is retired from active operations.
    /// It remains historical rather than current.
    /// </summary>
    Archived = 6
}
