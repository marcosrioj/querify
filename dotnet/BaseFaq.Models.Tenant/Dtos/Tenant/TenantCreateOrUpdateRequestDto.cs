using BaseFaq.Models.Tenant.Enums;

namespace BaseFaq.Models.Tenant.Dtos.Tenant;

public class TenantCreateOrUpdateRequestDto
{
    public Guid? TenantId { get; set; }
    public required string Name { get; set; }
    public required TenantEdition Edition { get; set; }
}
