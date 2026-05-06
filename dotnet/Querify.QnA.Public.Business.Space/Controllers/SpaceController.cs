using Querify.Common.Infrastructure.Core.Attributes;
using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Space;
using Querify.QnA.Public.Business.Space.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Querify.QnA.Public.Business.Space.Controllers;

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

    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(SpaceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken token)
    {
        return Ok(await spaceService.GetBySlug(slug, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<SpaceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] SpaceGetAllRequestDto requestDto,
        CancellationToken token)
    {
        return Ok(await spaceService.GetAll(requestDto, token));
    }
}
