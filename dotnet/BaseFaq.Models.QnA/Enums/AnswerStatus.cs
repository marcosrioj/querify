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
    /// The answer is waiting for moderation or editorial review.
    /// </summary>
    PendingReview = 1,

    /// <summary>
    /// The answer is visible for use.
    /// It may be published without yet being strongly validated.
    /// </summary>
    Published = 2,

    /// <summary>
    /// The answer has passed a stronger quality or governance check.
    /// This is the best candidate for accepted or canonical behavior.
    /// </summary>
    Validated = 3,

    /// <summary>
    /// The answer was reviewed and explicitly not accepted for use.
    /// The data remains for traceability.
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// The answer was once useful but should no longer be served as current guidance.
    /// Typical causes include product change or policy drift.
    /// </summary>
    Obsolete = 5,

    /// <summary>
    /// The answer is retired from active operations.
    /// It remains historical rather than current.
    /// </summary>
    Archived = 6
}
