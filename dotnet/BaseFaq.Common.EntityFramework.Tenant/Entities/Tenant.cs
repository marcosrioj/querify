using BaseFaq.Common.EntityFramework.Core.Entities;
using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Common.EntityFramework.Tenant.Entities;

public class Tenant : BaseEntity
{
    public const int MaxSlugLength = 128;
    public const int MaxNameLength = 128;
    public const int MaxConnectionStringLength = 1024;
    public const int MaxClientKeyLength = 128;
    public const string DefaultTenantName = "Default";

    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required TenantEdition Edition { get; set; }
    public required AppEnum App { get; set; }
    public required string ConnectionString { get; set; }
    public string? ClientKey { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<TenantAiProvider> AiProviders { get; set; } = [];

    public Guid UserId { get; set; }
}