using BaseFaq.Models.Common.Enums;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Commands.CreateTenantConnection;

public sealed class TenantConnectionsCreateTenantConnectionCommand : IRequest<Guid>
{
    public required AppEnum App { get; set; }
    public required string ConnectionString { get; set; }
    public required bool IsCurrent { get; set; }
}