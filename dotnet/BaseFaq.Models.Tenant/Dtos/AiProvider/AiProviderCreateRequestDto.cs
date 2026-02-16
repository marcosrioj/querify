using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Models.Tenant.Dtos.AiProvider;

public class AiProviderCreateRequestDto
{
    public required string Provider { get; set; }
    public required string Model { get; set; }
    public required string Prompt { get; set; }
    public required AiCommandType Command { get; set; }
}