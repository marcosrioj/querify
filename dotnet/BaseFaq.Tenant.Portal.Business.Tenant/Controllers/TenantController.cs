using BaseFaq.Models.Tenant.Dtos.Tenant;
using BaseFaq.Tenant.Portal.Business.Tenant.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Tenant.Portal.Business.Tenant.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant/tenants")]
public class TenantController(ITenantService tenantService) : ControllerBase
{
    [HttpGet("get-all")]
    [ProducesResponseType(typeof(List<TenantSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        var result = await tenantService.GetAll(token);
        return Ok(result);
    }

    [HttpPost("create-or-update")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateOrUpdate([FromBody] TenantCreateOrUpdateRequestDto dto,
        CancellationToken token)
    {
        var result = await tenantService.CreateOrUpdate(dto, token);
        return Ok(result);
    }

    [HttpPost("refresh-allowed-tenant-cache")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshAllowedTenantCache(CancellationToken token)
    {
        var result = await tenantService.RefreshAllowedTenantCache(token);
        return Ok(result);
    }

    [HttpGet("get-client-key")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientKey([FromQuery] Guid tenantId, CancellationToken token)
    {
        var result = await tenantService.GetClientKey(tenantId, token);
        return Ok(result);
    }

    [HttpPost("generate-new-client-key")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateNewClientKey([FromQuery] Guid tenantId, CancellationToken token)
    {
        var result = await tenantService.GenerateNewClientKey(tenantId, token);
        return Ok(result);
    }
}
