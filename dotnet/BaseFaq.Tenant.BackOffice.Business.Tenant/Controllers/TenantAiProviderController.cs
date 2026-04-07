using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.TenantAiProvider;
using BaseFaq.Tenant.BackOffice.Business.Tenant.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Tenant.BackOffice.Business.Tenant.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant/tenant-ai-providers")]
public class TenantAiProviderController(ITenantAiProviderService tenantAIProviderService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantAiProviderDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await tenantAIProviderService.GetById(id, token);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TenantAiProviderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] TenantAiProviderGetAllRequestDto requestDto,
        CancellationToken token)
    {
        var result = await tenantAIProviderService.GetAll(requestDto, token);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] TenantAiProviderCreateRequestDto dto, CancellationToken token)
    {
        var result = await tenantAIProviderService.Create(dto, token);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] TenantAiProviderUpdateRequestDto dto,
        CancellationToken token)
    {
        var result = await tenantAIProviderService.Update(id, dto, token);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await tenantAIProviderService.Delete(id, token);
        return NoContent();
    }
}
