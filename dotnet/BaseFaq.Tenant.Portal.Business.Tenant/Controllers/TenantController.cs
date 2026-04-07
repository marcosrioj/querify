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
    [HttpGet("GetAll")]
    [ProducesResponseType(typeof(List<TenantSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        var result = await tenantService.GetAll(token);
        return Ok(result);
    }

    [HttpPost("CreateOrUpdate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateOrUpdate([FromBody] TenantCreateOrUpdateRequestDto dto,
        CancellationToken token)
    {
        var result = await tenantService.CreateOrUpdate(dto, token);
        return Ok(result);
    }

    [HttpPost("RefreshAllowedTenantCache")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshAllowedTenantCache([FromQuery] Guid tenantId, CancellationToken token)
    {
        var result = await tenantService.RefreshAllowedTenantCache(tenantId, token);
        return Ok(result);
    }

    [HttpGet("GetClientKey")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientKey([FromQuery] Guid tenantId, CancellationToken token)
    {
        var result = await tenantService.GetClientKey(tenantId, token);
        return Ok(result);
    }

    [HttpPost("GenerateNewClientKey")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateNewClientKey([FromQuery] Guid tenantId, CancellationToken token)
    {
        var result = await tenantService.GenerateNewClientKey(tenantId, token);
        return Ok(result);
    }

    [HttpPost("SetAiProviderCredentials")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetAiProviderCredentials(
        [FromBody] TenantSetAiProviderCredentialsRequestDto dto,
        CancellationToken token)
    {
        var result = await tenantService.SetAiProviderCredentials(dto, token);
        return Ok(result);
    }
}
