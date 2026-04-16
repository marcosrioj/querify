using BaseFaq.Models.Common.Dtos;
using BaseFaq.Models.QnA.Dtos.KnowledgeSource;
using BaseFaq.QnA.Portal.Business.KnowledgeSource.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.QnA.Portal.Business.KnowledgeSource.Controllers;

[Authorize]
[ApiController]
[Route("api/qna/knowledge-source")]
public class KnowledgeSourceController(IKnowledgeSourceService knowledgeSourceService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(KnowledgeSourceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken token)
    {
        return Ok(await knowledgeSourceService.GetById(id, token));
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<KnowledgeSourceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] KnowledgeSourceGetAllRequestDto requestDto,
        CancellationToken token)
    {
        return Ok(await knowledgeSourceService.GetAll(requestDto, token));
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] KnowledgeSourceCreateRequestDto dto, CancellationToken token)
    {
        return StatusCode(StatusCodes.Status201Created, await knowledgeSourceService.Create(dto, token));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] KnowledgeSourceUpdateRequestDto dto,
        CancellationToken token)
    {
        return Ok(await knowledgeSourceService.Update(id, dto, token));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        await knowledgeSourceService.Delete(id, token);
        return NoContent();
    }
}