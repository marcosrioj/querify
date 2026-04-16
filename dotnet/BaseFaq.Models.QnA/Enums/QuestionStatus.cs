namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Defines the lifecycle of a question thread.
/// </summary>
public enum QuestionStatus
{
    /// <summary>
    /// The question exists but is still being prepared.
    /// It is not ready for operational or public use.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The question is waiting for moderation or editorial review.
    /// It has entered workflow but is not yet approved.
    /// </summary>
    PendingReview = 1,

    /// <summary>
    /// The question is live and actively unresolved or under discussion.
    /// Users can still contribute or the team can continue refining the answer.
    /// </summary>
    Open = 2,

    /// <summary>
    /// At least one answer exists that is good enough to resolve the question operationally.
    /// It may still need stronger validation or future refresh.
    /// </summary>
    Answered = 3,

    /// <summary>
    /// The question has a reviewed and trusted resolution.
    /// This is the strongest quality state before archival.
    /// </summary>
    Validated = 4,

    /// <summary>
    /// The question cannot be handled only inside the Q&amp;A flow and needs escalation.
    /// Typical cases include compliance, billing, account access, or product incidents.
    /// </summary>
    Escalated = 5,

    /// <summary>
    /// The question is not the canonical thread and should point to another question.
    /// This prevents knowledge fragmentation.
    /// </summary>
    Duplicate = 6,

    /// <summary>
    /// The question is no longer active in the knowledge surface.
    /// Historical data may still be preserved.
    /// </summary>
    Archived = 7
}
