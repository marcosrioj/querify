namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Describes lifecycle and signal events recorded in the question activity journal.
/// </summary>
public enum ActivityKind
{
    /// <summary>
    /// A question was created in the canonical Q&amp;A knowledge base.
    /// </summary>
    QuestionCreated = 18,

    /// <summary>
    /// Editable question fields changed without representing a lifecycle status transition.
    /// </summary>
    QuestionUpdated = 19,

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
    /// An answer was created for a canonical Q&amp;A question.
    /// </summary>
    AnswerCreated = 28,

    /// <summary>
    /// Editable answer fields changed without representing a lifecycle status transition.
    /// </summary>
    AnswerUpdated = 29,

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
    FeedbackReceived = 40,

    /// <summary>
    /// An answer-level ranking vote was received.
    /// </summary>
    VoteReceived = 41,
}
