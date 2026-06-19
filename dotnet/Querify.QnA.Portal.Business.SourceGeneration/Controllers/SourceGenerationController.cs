using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.SourceGeneration;
using Querify.QnA.Portal.Business.SourceGeneration.Abstractions;

namespace Querify.QnA.Portal.Business.SourceGeneration.Controllers;

[Authorize]
[ApiController]
[Route("api/qna")]
public sealed class SourceGenerationController(ISourceGenerationService sourceGenerationService) : ControllerBase
{
    [HttpPost("source/{sourceId:guid}/generate-space")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GenerateSpace(Guid sourceId, [FromBody] SourceGenerateSpaceRequestDto request,
        CancellationToken token)
    {
        var runId = await sourceGenerationService.GenerateSpace(sourceId, request, token);
        return Accepted($"/api/qna/source-generation/{runId}", runId);
    }

    [HttpGet("source-generation/{runId:guid}")]
    [ProducesResponseType(typeof(SourceGenerationRunDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRun(Guid runId, CancellationToken token)
    {
        return Ok(await sourceGenerationService.GetRun(runId, token));
    }

    [HttpGet("source/{sourceId:guid}/generation-runs")]
    [ProducesResponseType(typeof(PagedResultDto<SourceGenerationRunSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRuns(Guid sourceId, [FromQuery] int skipCount = 0,
        [FromQuery] int maxResultCount = 20, CancellationToken token = default)
    {
        return Ok(await sourceGenerationService.GetRuns(sourceId, skipCount, maxResultCount, token));
    }
}
