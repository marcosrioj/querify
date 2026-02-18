using BaseFaq.Models.Common.Enums;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.UpdateTenantConnection;

public sealed class TenantConnectionsUpdateTenantConnectionCommand : IRequest
{
    public required Guid Id { get; set; }
    public required AppEnum App { get; set; }
    public required string ConnectionString { get; set; }
    public required bool IsCurrent { get; set; }
}