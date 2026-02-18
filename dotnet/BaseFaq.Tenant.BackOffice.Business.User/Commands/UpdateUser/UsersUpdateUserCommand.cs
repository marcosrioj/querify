using BaseFaq.Models.User.Enums;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.User.Commands.UpdateUser;

public sealed class UsersUpdateUserCommand : IRequest
{
    public required Guid Id { get; set; }
    public required string GivenName { get; set; }
    public string? SurName { get; set; }
    public required string Email { get; set; }
    public required string ExternalId { get; set; }
    public string? PhoneNumber { get; set; }
    public required UserRoleType Role { get; set; }
}