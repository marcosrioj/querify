using Querify.Models.Common.Enums;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenantConnection;

public sealed class TenantConnectionsUpdateTenantConnectionCommand : IRequest
{
    public required Guid Id { get; set; }
    public required ModuleEnum Module { get; set; }
    public required string ConnectionString { get; set; }
    public required bool IsCurrent { get; set; }
}