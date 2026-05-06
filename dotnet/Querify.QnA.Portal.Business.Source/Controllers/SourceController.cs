using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Source;
using Querify.QnA.Portal.Business.Source.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Querify.QnA.Portal.Business.Source.Controllers;

[Authorize]
[ApiController]
[Route("api/qna/source")]
public class SourceController(ISourceService sourceService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SourceDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await sourceService.GetById(id, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<SourceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] SourceGetAllRequestDto requestDto,
        CancellationToken token)
    {
        return Ok(await sourceService.GetAll(requestDto, token));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] SourceCreateRequestDto dto, CancellationToken token)
    {
        return StatusCode(StatusCodes.Status201Created, await sourceService.Create(dto, token));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SourceUpdateRequestDto dto,
        CancellationToken token)
    {
        return Ok(await sourceService.Update(id, dto, token));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await sourceService.Delete(id, token);
        return NoContent();
    }
}
