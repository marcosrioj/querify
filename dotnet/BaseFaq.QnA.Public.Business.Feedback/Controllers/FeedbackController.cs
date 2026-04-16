using BaseFaq.Common.Infrastructure.Core.Attributes;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.QnA.Public.Business.Feedback.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.QnA.Public.Business.Feedback.Controllers;

[ApiController]
[SkipTenantAccessValidation]
[Route("api/qna/feedback")]
public class FeedbackController(IFeedbackService feedbackService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Feedback([FromBody] QuestionFeedbackCreateRequestDto dto, CancellationToken token)
    {
        return StatusCode(StatusCodes.Status201Created, await feedbackService.Create(dto, token));
    }
}
