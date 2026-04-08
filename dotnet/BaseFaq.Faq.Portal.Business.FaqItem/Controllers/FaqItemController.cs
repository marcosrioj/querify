using BaseFaq.Common.Infrastructure.ApiErrorHandling.Exception;
using BaseFaq.Faq.Portal.Business.FaqItem.Commands.CreateFaqItem;
using BaseFaq.Faq.Portal.Business.FaqItem.Commands.DeleteFaqItem;
using BaseFaq.Faq.Portal.Business.FaqItem.Commands.UpdateFaqItem;
using BaseFaq.Faq.Portal.Business.FaqItem.Queries.GetFaqItem;
using BaseFaq.Faq.Portal.Business.FaqItem.Queries.GetFaqItemList;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.FaqItem;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BaseFaq.Faq.Portal.Business.FaqItem.Controllers;

[Authorize]
[ApiController]
[Route("api/faqs/faq-item")]
public class FaqItemController(IMediator mediator) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FaqItemDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await mediator.Send(new FaqItemsGetFaqItemQuery { Id = id }, token);
        if (result is null)
        {
            throw new ApiErrorException(
                $"FAQ item '{id}' was not found.",
                errorCode: (int)HttpStatusCode.NotFound);
        }

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<FaqItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] FaqItemGetAllRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);
        var result = await mediator.Send(new FaqItemsGetFaqItemListQuery { Request = requestDto }, token);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] FaqItemCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var id = await mediator.Send(new FaqItemsCreateFaqItemCommand
        {
            Question = dto.Question,
            ShortAnswer = dto.ShortAnswer,
            Answer = dto.Answer,
            AdditionalInfo = dto.AdditionalInfo,
            CtaTitle = dto.CtaTitle,
            CtaUrl = dto.CtaUrl,
            Sort = dto.Sort,
            IsActive = dto.IsActive,
            FaqId = dto.FaqId,
            ContentRefId = dto.ContentRefId
        }, token);

        return StatusCode(StatusCodes.Status201Created, id);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] FaqItemUpdateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await mediator.Send(new FaqItemsUpdateFaqItemCommand
        {
            Id = id,
            Question = dto.Question,
            ShortAnswer = dto.ShortAnswer,
            Answer = dto.Answer,
            AdditionalInfo = dto.AdditionalInfo,
            CtaTitle = dto.CtaTitle,
            CtaUrl = dto.CtaUrl,
            Sort = dto.Sort,
            IsActive = dto.IsActive,
            FaqId = dto.FaqId,
            ContentRefId = dto.ContentRefId
        }, token);

        return Ok(id);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await mediator.Send(new FaqItemsDeleteFaqItemCommand { Id = id }, token);
        return NoContent();
    }
}