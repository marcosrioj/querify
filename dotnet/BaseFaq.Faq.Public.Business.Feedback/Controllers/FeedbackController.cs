using BaseFaq.Faq.Public.Business.Feedback.Abstractions;
using BaseFaq.Common.Infrastructure.Core.Attributes;
using BaseFaq.Models.Faq.Dtos.Feedback;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Faq.Public.Business.Feedback.Controllers;

[ApiController]
[SkipTenantAccessValidation]
[Route("api/faqs/feedback")]
public class FeedbackController(IFeedbackService feedbackService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Feedback([FromBody] FeedbackCreateRequestDto dto, CancellationToken token)
    {
        var result = await feedbackService.Feedback(dto, token);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}