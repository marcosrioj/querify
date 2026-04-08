namespace BaseFaq.Models.User.Dtos.User;

public class UserProfileDto
{
    public required string GivenName { get; set; }
    public string? SurName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public string? TimeZone { get; set; }
}
