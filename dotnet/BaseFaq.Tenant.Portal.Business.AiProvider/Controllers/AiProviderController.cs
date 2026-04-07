using BaseFaq.Models.Tenant.Dtos.AiProvider;
using BaseFaq.Tenant.Portal.Business.AiProvider.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Tenant.Portal.Business.AiProvider.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant/aiproviders")]
public class AiProviderController(IAiProviderService aiProviderService) : ControllerBase
{
    [HttpGet("GetAll")]
    [ProducesResponseType(typeof(List<AiProviderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        var result = await aiProviderService.GetAll(token);
        return Ok(result);
    }
}
