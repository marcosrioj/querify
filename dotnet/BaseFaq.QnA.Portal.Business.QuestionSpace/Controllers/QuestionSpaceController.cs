using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.QnA.Portal.Business.QuestionSpace.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.QnA.Portal.Business.QuestionSpace.Controllers;

[Authorize]
[ApiController]
[Route("api/qna/question-space")]
public class QuestionSpaceController(IQuestionSpaceService questionSpaceService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuestionSpaceDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await questionSpaceService.GetById(id, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<QuestionSpaceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] QuestionSpaceGetAllRequestDto requestDto, CancellationToken token)
    {
        return Ok(await questionSpaceService.GetAll(requestDto, token));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] QuestionSpaceCreateRequestDto dto, CancellationToken token)
    {
        return StatusCode(StatusCodes.Status201Created, await questionSpaceService.Create(dto, token));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] QuestionSpaceUpdateRequestDto dto, CancellationToken token)
    {
        return Ok(await questionSpaceService.Update(id, dto, token));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await questionSpaceService.Delete(id, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/topic")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddTopic(Guid id, [FromBody] QuestionSpaceTopicCreateRequestDto dto, CancellationToken token)
    {
        dto.QuestionSpaceId = id;
        return StatusCode(StatusCodes.Status201Created, await questionSpaceService.AddTopic(dto, token));
    }

    [HttpDelete("{id:guid}/topic/{topicId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveTopic(Guid id, Guid topicId, CancellationToken token)
    {
        await questionSpaceService.RemoveTopic(id, topicId, token);
        return NoContent();
    }

    [HttpPost("{id:guid}/source")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddSource(Guid id, [FromBody] QuestionSpaceSourceCreateRequestDto dto, CancellationToken token)
    {
        dto.QuestionSpaceId = id;
        return StatusCode(StatusCodes.Status201Created, await questionSpaceService.AddCuratedSource(dto, token));
    }

    [HttpDelete("{id:guid}/source/{knowledgeSourceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveSource(Guid id, Guid knowledgeSourceId, CancellationToken token)
    {
        await questionSpaceService.RemoveCuratedSource(id, knowledgeSourceId, token);
        return NoContent();
    }
}
