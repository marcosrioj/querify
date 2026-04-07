using BaseFaq.Models.Tenant.Dtos.TenantUser;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant/tenants/{tenantId:guid}/tenant-users")]
public class TenantUserController(ITenantUserService tenantUserService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<TenantUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(Guid tenantId, CancellationToken token)
    {
        var result = await tenantUserService.GetAll(tenantId, token);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(Guid tenantId, [FromBody] TenantUserCreateRequestDto dto, CancellationToken token)
    {
        var result = await tenantUserService.Create(tenantId, dto, token);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid tenantId, Guid id, [FromBody] TenantUserUpdateRequestDto dto, CancellationToken token)
    {
        var result = await tenantUserService.Update(tenantId, id, dto, token);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid tenantId, Guid id, CancellationToken token)
    {
        await tenantUserService.Delete(tenantId, id, token);
        return NoContent();
    }
}
