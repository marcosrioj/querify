using Querify.Models.User.Enums;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.User.Commands.CreateUser;

public sealed class UsersCreateUserCommand : IRequest<Guid>
{
    public required string GivenName { get; set; }
    public string? SurName { get; set; }
    public required string Email { get; set; }
    public required string ExternalId { get; set; }
    public string? PhoneNumber { get; set; }
    public required UserRoleType Role { get; set; }
}