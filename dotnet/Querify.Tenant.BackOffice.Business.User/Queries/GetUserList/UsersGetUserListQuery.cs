using Querify.Models.Common.Dtos;
using Querify.Models.User.Dtos.User;
using MediatR;

namespace Querify.Tenant.BackOffice.Business.User.Queries.GetUserList;

public sealed class UsersGetUserListQuery : IRequest<PagedResultDto<UserDto>>
{
    public required UserGetAllRequestDto Request { get; set; }
}