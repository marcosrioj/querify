using BaseFaq.Faq.Portal.Business.Feedback.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Feedback;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Faq.Portal.Business.Feedback.Controllers;

[Authorize]
[ApiController]
[Route("api/faqs/feedback")]
public class FeedbackController(IFeedbackService feedbackService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FeedbackDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await feedbackService.GetById(id, token);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<FeedbackDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] FeedbackGetAllRequestDto requestDto, CancellationToken token)
    {
        var result = await feedbackService.GetAll(requestDto, token);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] FeedbackCreateRequestDto dto, CancellationToken token)
    {
        var result = await feedbackService.Create(dto, token);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] FeedbackUpdateRequestDto dto, CancellationToken token)
    {
        var result = await feedbackService.Update(id, dto, token);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await feedbackService.Delete(id, token);
        return NoContent();
    }
}