using BaseFaq.Common.Infrastructure.Core.Attributes;
using BaseFaq.Faq.Public.Business.Vote.Abstractions;
using BaseFaq.Models.Faq.Dtos.Vote;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.Faq.Public.Business.Vote.Controllers;

[ApiController]
[SkipTenantAccessValidation]
[Route("api/faqs/vote")]
public class VoteController(IVoteService voteService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Vote([FromBody] VoteCreateRequestDto dto, CancellationToken token)
    {
        var result = await voteService.Vote(dto, token);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}
