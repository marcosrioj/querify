namespace BaseFaq.SupportCopilot.Common.Persistence.SupportCopilotDb.Enums;

/// <summary>
/// Identifies who wrote a support conversation message.
/// </summary>
public enum MessageActorKind
{
    User = 1,
    Copilot = 2,
    Agent = 3,
    System = 4
}
