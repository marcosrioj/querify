using Querify.Models.Common.Enums;
using Querify.Models.Tenant.Enums;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Commands.CreateTenant;

public sealed class TenantsCreateTenantCommand : IRequest<Guid>
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required TenantEdition Edition { get; set; }
    public required ModuleEnum Module { get; set; }
    public required string ConnectionString { get; set; }
    public required bool IsActive { get; set; }
    public required Guid UserId { get; set; }
}