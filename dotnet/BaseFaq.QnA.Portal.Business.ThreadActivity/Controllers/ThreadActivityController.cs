using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.ThreadActivity;
using BaseFaq.QnA.Portal.Business.ThreadActivity.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.QnA.Portal.Business.Controllers;

[Authorize]
[ApiController]
[Route("api/qna/thread-activity")]
public class ThreadActivityController(IThreadActivityService threadActivityService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ThreadActivityDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await threadActivityService.GetById(id, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ThreadActivityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] ThreadActivityGetAllRequestDto requestDto, CancellationToken token)
    {
        return Ok(await threadActivityService.GetAll(requestDto, token));
    }
}
