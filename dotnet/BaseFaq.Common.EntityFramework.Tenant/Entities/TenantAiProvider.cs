using BaseFaq.Common.EntityFramework.Core.Entities;

namespace BaseFaq.Common.EntityFramework.Tenant.Entities;

public class TenantAiProvider : BaseEntity
{
    public const int MaxAiProviderKeyLength = 255;

    public required Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public required Guid AiProviderId { get; set; }
    public AiProvider AiProvider { get; set; } = null!;

    public required string AiProviderKey { get; set; }
}