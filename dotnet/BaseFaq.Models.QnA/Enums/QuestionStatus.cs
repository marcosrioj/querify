namespace BaseFaq.Models.QnA.Enums;

/// <summary>
/// Defines the lifecycle of a question thread.
/// </summary>
public enum QuestionStatus
{
    /// <summary>
    /// The question exists but is still being prepared.
    /// It is not ready for normal operational or public use.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The question is active in the knowledge surface and can receive answers or user interaction.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The question is not the canonical thread and should point to another question.
    /// This prevents knowledge fragmentation.
    /// </summary>
    Duplicate = 2,

    /// <summary>
    /// The question is no longer active in the knowledge surface.
    /// Historical data may still be preserved.
    /// </summary>
    Archived = 3
}
