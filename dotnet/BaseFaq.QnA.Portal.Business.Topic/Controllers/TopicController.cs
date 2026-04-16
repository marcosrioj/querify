using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Topic;
using BaseFaq.QnA.Portal.Business.Topic.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.QnA.Portal.Business.Topic.Controllers;

[Authorize]
[ApiController]
[Route("api/qna/topic")]
public class TopicController(ITopicService topicService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TopicDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await topicService.GetById(id, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TopicDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] TopicGetAllRequestDto requestDto, CancellationToken token)
    {
        return Ok(await topicService.GetAll(requestDto, token));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] TopicCreateRequestDto dto, CancellationToken token)
    {
        return StatusCode(StatusCodes.Status201Created, await topicService.Create(dto, token));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] TopicUpdateRequestDto dto, CancellationToken token)
    {
        return Ok(await topicService.Update(id, dto, token));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await topicService.Delete(id, token);
        return NoContent();
    }
}