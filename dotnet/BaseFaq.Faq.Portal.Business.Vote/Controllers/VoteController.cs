using BaseFaq.Faq.Portal.Business.Vote.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Vote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Faq.Portal.Business.Vote.Controllers;

[Authorize]
[ApiController]
[Route("api/faqs/vote")]
public class VoteController(IVoteService voteService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VoteDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await voteService.GetById(id, token);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<VoteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] VoteGetAllRequestDto requestDto, CancellationToken token)
    {
        var result = await voteService.GetAll(requestDto, token);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] VoteCreateRequestDto dto, CancellationToken token)
    {
        var result = await voteService.Create(dto, token);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] VoteUpdateRequestDto dto, CancellationToken token)
    {
        var result = await voteService.Update(id, dto, token);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await voteService.Delete(id, token);
        return NoContent();
    }
}
