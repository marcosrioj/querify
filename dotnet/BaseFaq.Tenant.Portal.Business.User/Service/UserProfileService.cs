using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Models.User.Dtos.User;
using BaseFaq.Tenant.Portal.Business.User.Abstractions;
using BaseFaq.Tenant.Portal.Business.User.Commands.UpdateUserProfile;
using BaseFaq.Tenant.Portal.Business.User.Queries.GetUserProfile;
using MediatR;

namespace BaseFaq.Tenant.Portal.Business.User.Service;

public class UserProfileService(IMediator mediator) : IUserProfileService
{
    public async Task<UserProfileDto> GetUserProfile(CancellationToken token)
    {
        var result = await mediator.Send(new UsersGetUserProfileQuery(), token);
        if (result is null)
        {
            throw new ApiErrorException("Current user profile was not found.", errorCode: (int)HttpStatusCode.NotFound);
        }

        return result;
    }

    public async Task<bool> UpdateUserProfile(UserProfileUpdateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new UsersUpdateUserProfileCommand
        {
            GivenName = requestDto.GivenName,
            SurName = requestDto.SurName,
            PhoneNumber = requestDto.PhoneNumber
        };

        await mediator.Send(command, token);
        return true;
    }
}