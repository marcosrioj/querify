using Querify.Models.Common.Enums;

namespace Querify.Models.Tenant.Dtos.TenantConnection;

public class TenantConnectionCreateRequestDto
{
    public required ModuleEnum Module { get; set; }
    public required string ConnectionString { get; set; }
    public required bool IsCurrent { get; set; }
}