using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Activity;
using Querify.QnA.Portal.Business.Activity.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Querify.QnA.Portal.Business.Activity.Controllers;

[Authorize]
[ApiController]
[Route("api/qna/activity")]
public class ActivityController(IActivityService activityService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ActivityDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await activityService.GetById(id, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ActivityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] ActivityGetAllRequestDto requestDto,
        CancellationToken token)
    {
        return Ok(await activityService.GetAll(requestDto, token));
    }
}