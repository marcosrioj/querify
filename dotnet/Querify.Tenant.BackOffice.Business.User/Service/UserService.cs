using System.Net;
using Querify.Common.Infrastructure.Core.Abstractions;
using Querify.Models.Common.Enums;
using Querify.Models.Common.Dtos;
using Querify.Models.User.Dtos.User;
using Querify.Common.Infrastructure.ApiErrorHandling.Exception;
using MediatR;
using Querify.Tenant.BackOffice.Business.User.Abstractions;
using Querify.Tenant.BackOffice.Business.User.Commands.CreateUser;
using Querify.Tenant.BackOffice.Business.User.Commands.DeleteUser;
using Querify.Tenant.BackOffice.Business.User.Commands.UpdateUser;
using Querify.Tenant.BackOffice.Business.User.Queries.GetUser;
using Querify.Tenant.BackOffice.Business.User.Queries.GetUserList;

namespace Querify.Tenant.BackOffice.Business.User.Service;

public class UserService(IMediator mediator) : IUserService
{
    public async Task<Guid> Create(UserCreateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new UsersCreateUserCommand
        {
            GivenName = requestDto.GivenName,
            SurName = requestDto.SurName,
            Email = requestDto.Email,
            ExternalId = requestDto.ExternalId,
            PhoneNumber = requestDto.PhoneNumber,
            Role = requestDto.Role
        };

        return await mediator.Send(command, token);
    }

    public Task<PagedResultDto<UserDto>> GetAll(UserGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return mediator.Send(new UsersGetUserListQuery { Request = requestDto }, token);
    }

    public Task Delete(Guid id, CancellationToken token)
    {
        return mediator.Send(new UsersDeleteUserCommand { Id = id }, token);
    }

    public async Task<UserDto> GetById(Guid id, CancellationToken token)
    {
        var result = await mediator.Send(new UsersGetUserQuery { Id = id }, token);
        if (result is null)
        {
            throw new ApiErrorException(
                $"User '{id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return result;
    }

    public async Task<Guid> Update(Guid id, UserUpdateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        var command = new UsersUpdateUserCommand
        {
            Id = id,
            GivenName = requestDto.GivenName,
            SurName = requestDto.SurName,
            Email = requestDto.Email,
            ExternalId = requestDto.ExternalId,
            PhoneNumber = requestDto.PhoneNumber,
            Role = requestDto.Role
        };

        await mediator.Send(command, token);
        return id;
    }
}