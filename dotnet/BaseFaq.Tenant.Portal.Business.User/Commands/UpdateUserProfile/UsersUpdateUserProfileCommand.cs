using MediatR;

namespace BaseFaq.Tenant.Portal.Business.User.Commands.UpdateUserProfile;

public sealed class UsersUpdateUserProfileCommand : IRequest
{
    public required string GivenName { get; set; }
    public string? SurName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? TimeZone { get; set; }
}
