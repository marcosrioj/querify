using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.QnA.Portal.Business.Answer.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.QnA.Portal.Business.Answer.Controllers;

[Authorize]
[ApiController]
[Route("api/qna/answer")]
public class AnswerController(IAnswerService answerService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AnswerDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await answerService.GetById(id, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<AnswerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] AnswerGetAllRequestDto requestDto, CancellationToken token)
    {
        return Ok(await answerService.GetAll(requestDto, token));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] AnswerCreateRequestDto dto, CancellationToken token)
    {
        return StatusCode(StatusCodes.Status201Created, await answerService.Create(dto, token));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AnswerUpdateRequestDto dto, CancellationToken token)
    {
        return Ok(await answerService.Update(id, dto, token));
    }

    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken token)
    {
        return Accepted(await answerService.Activate(id, token));
    }

    [HttpPost("{id:guid}/retire")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Retire(Guid id, CancellationToken token)
    {
        return Accepted(await answerService.Retire(id, token));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await answerService.Delete(id, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/source")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddSource(Guid id, [FromBody] AnswerSourceLinkCreateRequestDto dto,
        CancellationToken token)
    {
        dto.AnswerId = id;
        return StatusCode(StatusCodes.Status201Created, await answerService.AddSource(dto, token));
    }

    [HttpDelete("{id:guid}/source/{sourceLinkId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveSource(Guid id, Guid sourceLinkId, CancellationToken token)
    {
        await answerService.RemoveSource(id, sourceLinkId, token);
        return NoContent();
    }
}
