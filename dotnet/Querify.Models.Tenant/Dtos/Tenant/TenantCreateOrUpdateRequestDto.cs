using Querify.Models.Tenant.Enums;

namespace Querify.Models.Tenant.Dtos.Tenant;

public class TenantCreateOrUpdateRequestDto
{
    public Guid? TenantId { get; set; }
    public required string Name { get; set; }
    public required TenantEdition Edition { get; set; }
}
