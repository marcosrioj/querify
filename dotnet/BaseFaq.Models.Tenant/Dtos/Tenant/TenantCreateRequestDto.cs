using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Models.Tenant.Dtos.Tenant;

public class TenantCreateRequestDto
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required TenantEdition Edition { get; set; }
    public required AppEnum App { get; set; }
    public required string ConnectionString { get; set; }
    public required bool IsActive { get; set; }
    public required Guid UserId { get; set; }
}