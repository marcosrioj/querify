using System.Net;
using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Commands.CreateFaqItemAnswer;
using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Commands.DeleteFaqItemAnswer;
using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Commands.UpdateFaqItemAnswer;
using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Queries.GetFaqItemAnswer;
using BaseFaq.Faq.Portal.Business.FaqItemAnswer.Queries.GetFaqItemAnswerList;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.FaqItemAnswer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Faq.Portal.Business.FaqItemAnswer.Controllers;

[Authorize]
[ApiController]
[Route("api/faqs/faq-item-answer")]
public class FaqItemAnswerController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FaqItemAnswerDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await mediator.Send(new FaqItemAnswersGetFaqItemAnswerQuery { Id = id }, token);
        if (result is null)
        {
            throw new ApiErrorException(
                $"FAQ item answer '{id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<FaqItemAnswerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] FaqItemAnswerGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        var result = await mediator.Send(new FaqItemAnswersGetFaqItemAnswerListQuery { Request = requestDto }, token);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] FaqItemAnswerCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var id = await mediator.Send(new FaqItemAnswersCreateFaqItemAnswerCommand
        {
            ShortAnswer = dto.ShortAnswer,
            Answer = dto.Answer,
            Sort = dto.Sort,
            IsActive = dto.IsActive,
            FaqItemId = dto.FaqItemId
        }, token);

        return StatusCode(StatusCodes.Status201Created, id);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] FaqItemAnswerUpdateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await mediator.Send(new FaqItemAnswersUpdateFaqItemAnswerCommand
        {
            Id = id,
            ShortAnswer = dto.ShortAnswer,
            Answer = dto.Answer,
            Sort = dto.Sort,
            IsActive = dto.IsActive,
            FaqItemId = dto.FaqItemId
        }, token);

        return Ok(id);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await mediator.Send(new FaqItemAnswersDeleteFaqItemAnswerCommand { Id = id }, token);
        return NoContent();
    }
}
