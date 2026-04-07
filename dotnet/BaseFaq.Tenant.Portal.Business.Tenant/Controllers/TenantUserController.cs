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
    public async Task<IActionResult> GetAll([FromQuery] Guid tenantId, CancellationToken token)
    {
        var result = await tenantUserService.GetAll(tenantId, token);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Upsert([FromBody] TenantUserCreateRequestDto dto, CancellationToken token)
    {
        var result = await tenantUserService.Upsert(dto, token);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid tenantId, CancellationToken token)
    {
        await tenantUserService.Delete(tenantId, id, token);
        return NoContent();
    }
}
