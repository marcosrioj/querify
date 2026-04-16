using BaseFaq.Common.Infrastructure.Core.Attributes;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Space;
using BaseFaq.QnA.Public.Business.Space.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.QnA.Public.Business.Space.Controllers;

[ApiController]
[SkipTenantAccessValidation]
[Route("api/qna/space")]
public class SpaceController(ISpaceService spaceService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SpaceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await spaceService.GetById(id, token));
    }

    [HttpGet("by-key/{key}")]
    [ProducesResponseType(typeof(SpaceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByKey(string key, CancellationToken token)
    {
        return Ok(await spaceService.GetByKey(key, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<SpaceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] SpaceGetAllRequestDto requestDto,
        CancellationToken token)
    {
        return Ok(await spaceService.GetAll(requestDto, token));
    }
}