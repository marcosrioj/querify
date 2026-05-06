using Querify.Models.Common.Dtos;
using Querify.Models.User.Dtos.User;
using Querify.Tenant.BackOffice.Business.User.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Querify.Tenant.BackOffice.Business.User.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant/users")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await userService.GetById(id, token);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] UserGetAllRequestDto requestDto, CancellationToken token)
    {
        var result = await userService.GetAll(requestDto, token);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] UserCreateRequestDto dto, CancellationToken token)
    {
        var result = await userService.Create(dto, token);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequestDto dto, CancellationToken token)
    {
        var result = await userService.Update(id, dto, token);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await userService.Delete(id, token);
        return NoContent();
    }
}