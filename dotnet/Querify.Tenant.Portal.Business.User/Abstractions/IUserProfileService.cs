using Querify.Models.User.Dtos.User;

namespace Querify.Tenant.Portal.Business.User.Abstractions;

public interface IUserProfileService
{
    Task<UserProfileDto> GetUserProfile(CancellationToken token);
    Task<bool> UpdateUserProfile(UserProfileUpdateRequestDto requestDto, CancellationToken token);
}