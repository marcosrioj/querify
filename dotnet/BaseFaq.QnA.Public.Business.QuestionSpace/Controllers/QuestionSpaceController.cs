using BaseFaq.Common.Infrastructure.Core.Attributes;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.QuestionSpace;
using BaseFaq.QnA.Public.Business.QuestionSpace.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.QnA.Public.Business.QuestionSpace.Controllers;

[ApiController]
[SkipTenantAccessValidation]
[Route("api/qna/question-space")]
public class QuestionSpaceController(IQuestionSpaceService questionSpaceService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuestionSpaceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await questionSpaceService.GetById(id, token));
    }

    [HttpGet("by-key/{key}")]
    [ProducesResponseType(typeof(QuestionSpaceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByKey(string key, CancellationToken token)
    {
        return Ok(await questionSpaceService.GetByKey(key, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<QuestionSpaceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] QuestionSpaceGetAllRequestDto requestDto, CancellationToken token)
    {
        return Ok(await questionSpaceService.GetAll(requestDto, token));
    }
}
