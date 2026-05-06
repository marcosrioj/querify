using Querify.Common.Infrastructure.Core.Attributes;
using Querify.Models.Common.Dtos;
using Querify.Models.QnA.Dtos.Question;
using Querify.QnA.Public.Business.Question.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Querify.QnA.Public.Business.Question.Controllers;

[ApiController]
[SkipTenantAccessValidation]
[Route("api/qna/question")]
public class QuestionController(IQuestionService questionService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(QuestionDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] QuestionGetRequestDto requestDto,
        CancellationToken token)
    {
        return Ok(await questionService.GetById(id, requestDto, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<QuestionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] QuestionGetAllRequestDto requestDto, CancellationToken token)
    {
        return Ok(await questionService.GetAll(requestDto, token));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] QuestionCreateRequestDto dto, CancellationToken token)
    {
        return StatusCode(StatusCodes.Status201Created, await questionService.Create(dto, token));
    }

}
