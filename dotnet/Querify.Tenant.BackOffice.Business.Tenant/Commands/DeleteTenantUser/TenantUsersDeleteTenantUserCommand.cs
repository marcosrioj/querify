using MediatR;

namespace Querify.Tenant.BackOffice.Business.Tenant.Commands.DeleteTenantUser;

public sealed class TenantUsersDeleteTenantUserCommand : IRequest
{
    public required Guid TenantId { get; set; }
    public required Guid Id { get; set; }
}
