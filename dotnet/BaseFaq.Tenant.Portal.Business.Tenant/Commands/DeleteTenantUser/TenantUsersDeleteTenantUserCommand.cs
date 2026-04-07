using MediatR;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Commands.DeleteTenantUser;

public sealed class TenantUsersDeleteTenantUserCommand : IRequest
{
    public required Guid Id { get; set; }
}
