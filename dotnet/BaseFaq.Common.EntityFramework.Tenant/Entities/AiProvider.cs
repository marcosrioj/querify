using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Common.EntityFramework.Tenant.Entities;

public class AiProvider : BaseEntity
{
    public const int MaxProviderLength = 128;
    public const int MaxModelLength = 128;
    public const int MaxPromptLength = 5000;

    public required string Provider { get; set; }
    public required string Model { get; set; }
    public required string Prompt { get; set; }
    public required AiCommandType Command { get; set; }
}