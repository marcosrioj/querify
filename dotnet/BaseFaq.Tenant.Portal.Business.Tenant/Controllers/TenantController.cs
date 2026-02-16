using BaseFaq.Models.Tenant.Dtos.Tenant;
using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using BaseFaq.Models.Tenant.Enums;
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

    [HttpGet("GetClientKey")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientKey(CancellationToken token)
    {
        var result = await tenantService.GetClientKey(token);
        return Ok(result);
    }

    [HttpPost("GenerateNewClientKey")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GenerateNewClientKey(CancellationToken token)
    {
        var result = await tenantService.GenerateNewClientKey(token);
        return Ok(result);
    }

    [HttpGet("GetConfiguredAiProviders")]
    [ProducesResponseType(typeof(List<TenantAiProviderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfiguredAiProviders(CancellationToken token)
    {
        var result = await tenantService.GetConfiguredAiProviders(token);
        return Ok(result);
    }

    [HttpPost("SetAiProviderCredentials")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetAiProviderCredentials(
        [FromBody] TenantSetAiProviderCredentialsRequestDto dto,
        CancellationToken token)
    {
        await tenantService.SetAiProviderCredentials(dto, token);
        return NoContent();
    }

    [HttpGet("IsAiProviderKeyConfigured/{command:int}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsAiProviderKeyConfigured(AiCommandType command, CancellationToken token)
    {
        var result = await tenantService.IsAiProviderKeyConfigured(command, token);
        return Ok(result);
    }
}