using BaseFaq.Models.User.Dtos.User;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.User.Queries.GetUser;

public sealed class UsersGetUserQuery : IRequest<UserDto?>
{
    public required Guid Id { get; set; }
}