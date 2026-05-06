using MediatR;

namespace Querify.Tenant.BackOffice.Business.User.Commands.DeleteUser;

public sealed class UsersDeleteUserCommand : IRequest
{
    public required Guid Id { get; set; }
}