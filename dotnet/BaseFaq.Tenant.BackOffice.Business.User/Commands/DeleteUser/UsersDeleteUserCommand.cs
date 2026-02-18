using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.User.Commands.DeleteUser;

public sealed class UsersDeleteUserCommand : IRequest
{
    public required Guid Id { get; set; }
}