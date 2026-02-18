using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.User.Dtos.User;
using MediatR;

namespace BaseFaq.Tenant.BackOffice.Business.User.Queries.GetUserList;

public sealed class UsersGetUserListQuery : IRequest<PagedResultDto<UserDto>>
{
    public required UserGetAllRequestDto Request { get; set; }
}