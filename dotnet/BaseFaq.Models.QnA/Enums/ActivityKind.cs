namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Describes lifecycle and signal events recorded in the question activity journal.
/// </summary>
public enum ActivityKind
{
    /// <summary>
    /// A question entered draft status and is not ready for normal operational or public use.
    /// </summary>
    QuestionDraft = 20,

    /// <summary>
    /// A question entered active status and can receive answers or user interaction.
    /// </summary>
    QuestionActive = 21,

    /// <summary>
    /// A question entered archived status and is preserved as historical knowledge.
    /// </summary>
    QuestionArchived = 22,

    /// <summary>
    /// An answer entered draft status and is still being prepared.
    /// </summary>
    AnswerDraft = 30,

    /// <summary>
    /// An answer entered active status and is ready for operational use.
    /// </summary>
    AnswerActive = 31,

    /// <summary>
    /// An answer entered archived status and is no longer active.
    /// </summary>
    AnswerArchived = 32,

    /// <summary>
    /// A question-level usefulness signal was received.
    /// </summary>
    FeedbackReceived = 14,

    /// <summary>
    /// An answer-level ranking vote was received.
    /// </summary>
    VoteReceived = 15,
}
