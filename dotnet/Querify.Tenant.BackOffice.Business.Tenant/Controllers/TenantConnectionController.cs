using Querify.Models.Common.Dtos;
using Querify.Models.Tenant.Dtos.TenantConnection;
using Querify.Tenant.BackOffice.Business.Tenant.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Querify.Tenant.BackOffice.Business.Tenant.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant/tenant-connections")]
public class TenantConnectionController(ITenantConnectionService tenantConnectionService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantConnectionDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await tenantConnectionService.GetById(id, token);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TenantConnectionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] TenantConnectionGetAllRequestDto requestDto,
        CancellationToken token)
    {
        var result = await tenantConnectionService.GetAll(requestDto, token);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] TenantConnectionCreateRequestDto dto,
        CancellationToken token)
    {
        var result = await tenantConnectionService.Create(dto, token);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] TenantConnectionUpdateRequestDto dto,
        CancellationToken token)
    {
        var result = await tenantConnectionService.Update(id, dto, token);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await tenantConnectionService.Delete(id, token);
        return NoContent();
    }
}