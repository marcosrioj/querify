namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Describes operational events recorded in the thread journal.
/// </summary>
public enum ActivityKind
{
    /// <summary>
    /// A new question thread was created.
    /// </summary>
    QuestionCreated = 1,

    /// <summary>
    /// The question content or metadata changed.
    /// </summary>
    QuestionUpdated = 2,

    /// <summary>
    /// A question entered workflow, often from a public intake path.
    /// </summary>
    QuestionSubmitted = 3,

    /// <summary>
    /// A moderator or editor approved the question.
    /// </summary>
    QuestionApproved = 4,

    /// <summary>
    /// A moderator or editor rejected the question.
    /// </summary>
    QuestionRejected = 5,

    /// <summary>
    /// The question was redirected to another canonical question.
    /// </summary>
    QuestionMarkedDuplicate = 6,

    /// <summary>
    /// The thread required escalation outside the normal Q&amp;A resolution path.
    /// </summary>
    QuestionEscalated = 7,

    /// <summary>
    /// A new answer candidate was created.
    /// </summary>
    AnswerCreated = 8,

    /// <summary>
    /// An answer candidate was modified.
    /// </summary>
    AnswerUpdated = 9,

    /// <summary>
    /// An answer became visible for use.
    /// </summary>
    AnswerPublished = 10,

    /// <summary>
    /// An answer was chosen as the accepted resolution for the thread.
    /// </summary>
    AnswerAccepted = 11,

    /// <summary>
    /// An answer passed a stronger validation gate.
    /// </summary>
    AnswerValidated = 12,

    /// <summary>
    /// An answer was reviewed and rejected.
    /// </summary>
    AnswerRejected = 13,

    /// <summary>
    /// A thread-level usefulness signal was received.
    /// </summary>
    FeedbackReceived = 14,

    /// <summary>
    /// An answer-level ranking vote was received.
    /// </summary>
    VoteReceived = 15,

    /// <summary>
    /// An answer was retired from active use.
    /// </summary>
    AnswerRetired = 16
}
