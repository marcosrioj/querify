namespace BaseFaq.Models.User.Dtos.User;

public class UserProfileUpdateRequestDto
{
    public required string GivenName { get; set; }
    public string? SurName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Language { get; set; }
    public string? TimeZone { get; set; }
}
