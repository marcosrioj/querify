using System.Net;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using Querify.Models.User.Dtos.User;
using Querify.Tenant.Portal.Business.User.Abstractions;
using Querify.Tenant.Portal.Business.User.Commands.UpdateUserProfile;
using Querify.Tenant.Portal.Business.User.Queries.GetUserProfile;
using MediatR;

namespace Querify.Tenant.Portal.Business.User.Service;

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
            PhoneNumber = requestDto.PhoneNumber,
            Language = requestDto.Language,
            TimeZone = requestDto.TimeZone
        };

        await mediator.Send(command, token);
        return true;
    }
}
