namespace Querify.Models.QnA.Enums;

/// <summary>
/// Tracks the lifecycle of a source-to-space generation run.
/// </summary>
public enum SourceGenerationRunStatus
{
    /// <summary>
    /// The generation run has been accepted but has not started execution.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// The generation run is creating the draft QnA graph.
    /// </summary>
    Running = 6,

    /// <summary>
    /// The generation run finished and created a draft space.
    /// </summary>
    Completed = 11,

    /// <summary>
    /// The generation run failed validation or execution.
    /// </summary>
    Failed = 16
}
