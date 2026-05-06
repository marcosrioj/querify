using Querify.Models.Common.Dtos;
using Querify.Models.User.Dtos.User;

namespace Querify.Tenant.BackOffice.Business.User.Abstractions;

public interface IUserService
{
    Task<Guid> Create(UserCreateRequestDto requestDto, CancellationToken token);
    Task Delete(Guid id, CancellationToken token);
    Task<PagedResultDto<UserDto>> GetAll(UserGetAllRequestDto requestDto, CancellationToken token);
    Task<UserDto> GetById(Guid id, CancellationToken token);
    Task<Guid> Update(Guid id, UserUpdateRequestDto requestDto, CancellationToken token);
}