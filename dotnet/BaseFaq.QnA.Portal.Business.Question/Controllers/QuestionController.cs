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

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await questionService.Delete(id, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/tag")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddTag(Guid id, [FromBody] QuestionTagCreateRequestDto dto,
        CancellationToken token)
    {
        dto.QuestionId = id;
        return StatusCode(StatusCodes.Status201Created, await questionService.AddTag(dto, token));
    }

    [HttpDelete("{id:guid}/tag/{tagId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTag(Guid id, Guid tagId, CancellationToken token)
    {
        await questionService.RemoveTag(id, tagId, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/source")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddSource(Guid id, [FromBody] QuestionSourceLinkCreateRequestDto dto,
        CancellationToken token)
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
