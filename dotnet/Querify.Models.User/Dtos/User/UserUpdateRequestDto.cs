using Querify.Models.User.Enums;

namespace Querify.Models.User.Dtos.User;

public class UserUpdateRequestDto
{
    public required string GivenName { get; set; }
    public string? SurName { get; set; }
    public required string Email { get; set; }
    public required string ExternalId { get; set; }
    public string? PhoneNumber { get; set; }
    public required UserRoleType Role { get; set; }
}