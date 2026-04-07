using BaseFaq.Models.Tenant.Dtos.TenantUser;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant/tenant-users")]
public class TenantUserController(ITenantUserService tenantUserService) : ControllerBase
{
    [HttpGet("GetAll")]
    [ProducesResponseType(typeof(List<TenantUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        var result = await tenantUserService.GetAll(token);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] TenantUserCreateRequestDto dto, CancellationToken token)
    {
        var result = await tenantUserService.Create(dto, token);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] TenantUserUpdateRequestDto dto, CancellationToken token)
    {
        var result = await tenantUserService.Update(id, dto, token);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await tenantUserService.Delete(id, token);
        return NoContent();
    }
}
