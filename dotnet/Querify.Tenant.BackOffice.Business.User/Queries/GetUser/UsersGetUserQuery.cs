using Querify.Models.User.Dtos.User;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.User.Queries.GetUser;

public sealed class UsersGetUserQuery : IRequest<UserDto?>
{
    public required Guid Id { get; set; }
}