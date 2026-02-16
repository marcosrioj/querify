using BaseFaq.Models.Tenant.Enums;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenant;

public class TenantsUpdateTenantCommand : IRequest
{
    public required Guid Id { get; set; }
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required TenantEdition Edition { get; set; }
    public required string ConnectionString { get; set; }
    public required bool IsActive { get; set; }
    public required Guid UserId { get; set; }
}