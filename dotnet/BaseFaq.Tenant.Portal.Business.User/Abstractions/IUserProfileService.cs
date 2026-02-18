using BaseFaq.Models.User.Dtos.User;

namespace BaseFaq.Tenant.Portal.Business.User.Abstractions;

public interface IUserProfileService
{
    Task<UserProfileDto> GetUserProfile(CancellationToken token);
    Task<bool> UpdateUserProfile(UserProfileUpdateRequestDto requestDto, CancellationToken token);
}