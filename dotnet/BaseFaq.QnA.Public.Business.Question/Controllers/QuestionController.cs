using BaseFaq.Common.Infrastructure.Core.Attributes;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.Question;
using BaseFaq.QnA.Public.Business.Question.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.QnA.Public.Business.Question.Controllers;

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

    [HttpGet("by-key/{key}")]
    [ProducesResponseType(typeof(QuestionDetailDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByKey(string key, [FromQuery] QuestionGetRequestDto requestDto,
        CancellationToken token)
    {
        return Ok(await questionService.GetByKey(key, requestDto, token));
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