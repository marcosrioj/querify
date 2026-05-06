using MediatR;

namespace Querify.Tenant.Portal.Business.Tenant.Commands.GenerateNewClientKey;

public sealed class TenantsGenerateNewClientKeyCommand : IRequest<string>
{
    public required Guid TenantId { get; set; }
}
