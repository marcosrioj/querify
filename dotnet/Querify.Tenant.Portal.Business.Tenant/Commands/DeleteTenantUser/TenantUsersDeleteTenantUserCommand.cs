using MediatR;

namespace Querify.Tenant.Portal.Business.Tenant.Commands.DeleteTenantUser;

public sealed class TenantUsersDeleteTenantUserCommand : IRequest
{
    public required Guid TenantId { get; set; }
    public required Guid Id { get; set; }
}
