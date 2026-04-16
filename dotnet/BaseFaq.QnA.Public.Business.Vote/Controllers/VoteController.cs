using BaseFaq.Common.Infrastructure.Core.Attributes;
using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.QnA.Public.Business.Vote.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BaseFaq.QnA.Public.Business.Controllers;

[ApiController]
[SkipTenantAccessValidation]
[Route("api/qna/vote")]
public class VoteController(IVoteService voteService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Vote([FromBody] AnswerVoteCreateRequestDto dto, CancellationToken token)
    {
        return StatusCode(StatusCodes.Status201Created, await voteService.Create(dto, token));
    }
}
