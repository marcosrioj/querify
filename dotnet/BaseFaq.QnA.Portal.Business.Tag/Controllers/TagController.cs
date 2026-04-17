using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Tag;
using BaseFaq.QnA.Portal.Business.Tag.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.QnA.Portal.Business.Tag.Controllers;

[Authorize]
[ApiController]
[Route("api/qna/tag")]
public class TagController(ITagService tagService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await tagService.GetById(id, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] TagGetAllRequestDto requestDto, CancellationToken token)
    {
        return Ok(await tagService.GetAll(requestDto, token));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] TagCreateRequestDto dto, CancellationToken token)
    {
        return StatusCode(StatusCodes.Status201Created, await tagService.Create(dto, token));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] TagUpdateRequestDto dto, CancellationToken token)
    {
        return Ok(await tagService.Update(id, dto, token));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await tagService.Delete(id, token);
        return NoContent();
    }
}