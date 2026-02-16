using BaseFaq.Models.Common.Enums;
using BaseFaq.Models.Tenant.Enums;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenant;

public class TenantsCreateTenantCommand : IRequest<Guid>
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required TenantEdition Edition { get; set; }
    public required AppEnum App { get; set; }
    public required string ConnectionString { get; set; }
    public required bool IsActive { get; set; }
    public required Guid UserId { get; set; }
}