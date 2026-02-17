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
public class TenantAiProviderController(ITenantAiProviderService tenantAiProviderService) : ControllerBase
{
    [HttpGet("GetConfiguredAiProviders")]
    [ProducesResponseType(typeof(List<TenantAiProviderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfiguredAiProviders(CancellationToken token)
    {
        var result = await tenantAiProviderService.GetConfiguredAiProviders(token);
        return Ok(result);
    }

    [HttpGet("IsAiProviderKeyConfigured/{command:int}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> IsAiProviderKeyConfigured(AiCommandType command, CancellationToken token)
    {
        var result = await tenantAiProviderService.IsAiProviderKeyConfigured(command, token);
        return Ok(result);
    }
}