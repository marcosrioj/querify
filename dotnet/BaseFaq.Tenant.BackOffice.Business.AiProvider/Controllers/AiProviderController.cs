using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Tenant.Dtos.AiProvider;
using BaseFaq.Tenant.BackOffice.Business.AiProvider.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Tenant.BackOffice.Business.AiProvider.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant/aiproviders")]
public class AiProviderController(IAiProviderService aiProviderService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AiProviderDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await aiProviderService.GetById(id, token);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<AiProviderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] AiProviderGetAllRequestDto requestDto, CancellationToken token)
    {
        var result = await aiProviderService.GetAll(requestDto, token);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] AiProviderCreateRequestDto dto, CancellationToken token)
    {
        var result = await aiProviderService.Create(dto, token);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AiProviderUpdateRequestDto dto, CancellationToken token)
    {
        var result = await aiProviderService.Update(id, dto, token);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await aiProviderService.Delete(id, token);
        return NoContent();
    }
}