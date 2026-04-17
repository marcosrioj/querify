using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Space;
using BaseFaq.QnA.Portal.Business.Space.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.QnA.Portal.Business.Space.Controllers;

[Authorize]
[ApiController]
[Route("api/qna/space")]
public class SpaceController(ISpaceService spaceService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SpaceDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await spaceService.GetById(id, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<SpaceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] SpaceGetAllRequestDto requestDto,
        CancellationToken token)
    {
        return Ok(await spaceService.GetAll(requestDto, token));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] SpaceCreateRequestDto dto, CancellationToken token)
    {
        return StatusCode(StatusCodes.Status201Created, await spaceService.Create(dto, token));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SpaceUpdateRequestDto dto,
        CancellationToken token)
    {
        return Ok(await spaceService.Update(id, dto, token));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await spaceService.Delete(id, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/tag")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddTag(Guid id, [FromBody] SpaceTagCreateRequestDto dto,
        CancellationToken token)
    {
        dto.SpaceId = id;
        return StatusCode(StatusCodes.Status201Created, await spaceService.AddTag(dto, token));
    }

    [HttpDelete("{id:guid}/tag/{tagId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTag(Guid id, Guid tagId, CancellationToken token)
    {
        await spaceService.RemoveTag(id, tagId, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/source")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddSource(Guid id, [FromBody] SpaceSourceCreateRequestDto dto,
        CancellationToken token)
    {
        dto.SpaceId = id;
        return StatusCode(StatusCodes.Status201Created, await spaceService.AddCuratedSource(dto, token));
    }

    [HttpDelete("{id:guid}/source/{sourceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveSource(Guid id, Guid sourceId, CancellationToken token)
    {
        await spaceService.RemoveCuratedSource(id, sourceId, token);
        return NoContent();
    }
}