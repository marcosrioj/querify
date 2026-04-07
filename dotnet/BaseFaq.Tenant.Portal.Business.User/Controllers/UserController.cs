using BaseFaq.Models.User.Dtos.User;
using BaseFaq.Tenant.Portal.Business.User.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Tenant.Portal.Business.User.Controllers;

[Authorize]
[ApiController]
[Route("api/user")]
public class UserController(IUserProfileService userProfileService) : ControllerBase
{
    [HttpGet("UserProfile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UserProfile(CancellationToken token)
    {
        var result = await userProfileService.GetUserProfile(token);
        return Ok(result);
    }

    [HttpPut("UserProfile")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> UserProfile([FromBody] UserProfileUpdateRequestDto dto, CancellationToken token)
    {
        var result = await userProfileService.UpdateUserProfile(dto, token);
        return Ok(result);
    }
}
