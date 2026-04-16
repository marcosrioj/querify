using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.QnA.Portal.Business.Question.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.QnA.Portal.Business.Question.Controllers;

[Authorize]
[ApiController]
[Route("api/qna/question")]
public class QuestionController(IQuestionService questionService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuestionDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await questionService.GetById(id, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<QuestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] QuestionGetAllRequestDto requestDto, CancellationToken token)
    {
        return Ok(await questionService.GetAll(requestDto, token));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] QuestionCreateRequestDto dto, CancellationToken token)
    {
        return StatusCode(StatusCodes.Status201Created, await questionService.Create(dto, token));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] QuestionUpdateRequestDto dto, CancellationToken token)
    {
        return Ok(await questionService.Update(id, dto, token));
    }

    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken token)
    {
        return Accepted(await questionService.Submit(id, token));
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken token)
    {
        return Accepted(await questionService.Approve(id, token));
    }

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string? notes, CancellationToken token)
    {
        return Accepted(await questionService.Reject(id, notes, token));
    }

    [HttpPost("{id:guid}/escalate")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Escalate(Guid id, [FromBody] string? notes, CancellationToken token)
    {
        return Accepted(await questionService.Escalate(id, notes, token));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await questionService.Delete(id, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/topic")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddTopic(Guid id, [FromBody] QuestionTopicCreateRequestDto dto, CancellationToken token)
    {
        dto.QuestionId = id;
        return StatusCode(StatusCodes.Status201Created, await questionService.AddTopic(dto, token));
    }

    [HttpDelete("{id:guid}/topic/{topicId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTopic(Guid id, Guid topicId, CancellationToken token)
    {
        await questionService.RemoveTopic(id, topicId, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/source")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddSource(Guid id, [FromBody] QuestionSourceLinkCreateRequestDto dto, CancellationToken token)
    {
        dto.QuestionId = id;
        return StatusCode(StatusCodes.Status201Created, await questionService.AddSource(dto, token));
    }

    [HttpDelete("{id:guid}/source/{sourceLinkId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveSource(Guid id, Guid sourceLinkId, CancellationToken token)
    {
        await questionService.RemoveSource(id, sourceLinkId, token);
        return NoContent();
    }
}
