using Querify.Models.Common.Enums;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Commands.CreateTenantConnection;

public sealed class TenantConnectionsCreateTenantConnectionCommand : IRequest<Guid>
{
    public required ModuleEnum Module { get; set; }
    public required string ConnectionString { get; set; }
    public required bool IsCurrent { get; set; }
}