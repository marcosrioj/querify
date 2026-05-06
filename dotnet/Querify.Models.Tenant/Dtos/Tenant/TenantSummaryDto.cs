using Querify.Models.Common.Enums;
using Querify.Models.Tenant.Enums;

namespace Querify.Models.Tenant.Dtos.Tenant;

public class TenantSummaryDto
{
    public required Guid Id { get; set; }
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required TenantEdition Edition { get; set; }
    public required ModuleEnum Module { get; set; }
    public required bool IsActive { get; set; }
    public required TenantUserRoleType CurrentUserRole { get; set; }
}
