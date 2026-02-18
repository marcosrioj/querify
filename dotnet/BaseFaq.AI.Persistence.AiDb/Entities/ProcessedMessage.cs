using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.AI.Persistence.AiDb.Entities;

public class ProcessedMessage : BaseEntity
{
    public const int MaxHandlerNameLength = 256;
    public const int MaxMessageIdLength = 64;

    public required string HandlerName { get; set; }
    public required string MessageId { get; set; }
    public DateTime ProcessedUtc { get; set; }
}