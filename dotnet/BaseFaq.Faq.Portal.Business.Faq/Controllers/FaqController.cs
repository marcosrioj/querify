using BaseFaq.Faq.Portal.Business.Faq.Abstractions;
using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.Faq.Dtos.Faq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Faq.Portal.Business.Faq.Controllers;

[Authorize]
[ApiController]
[Route("api/faqs/faq")]
public class FaqController(IFaqService faqService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FaqDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        var result = await faqService.GetById(id, token);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<FaqDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] FaqGetAllRequestDto requestDto, CancellationToken token)
    {
        var result = await faqService.GetAll(requestDto, token);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] FaqCreateRequestDto dto, CancellationToken token)
    {
        var result = await faqService.Create(dto, token);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] FaqUpdateRequestDto dto, CancellationToken token)
    {
        var result = await faqService.Update(id, dto, token);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await faqService.Delete(id, token);
        return NoContent();
    }
}
