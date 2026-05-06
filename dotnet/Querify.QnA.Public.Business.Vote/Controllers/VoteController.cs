using Querify.Common.Infrastructure.Core.Attributes;
using Querify.Models.QnA.Dtos.Answer;
using Querify.QnA.Public.Business.Vote.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Querify.QnA.Public.Business.Vote.Controllers;

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