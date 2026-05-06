using Querify.Models.QnA.Dtos.Answer;
using Querify.QnA.Public.Business.Vote.Abstractions;
using Querify.QnA.Public.Business.Vote.Commands.CreateVote;
using MediatR;

namespace Querify.QnA.Public.Business.Vote.Service;

public sealed class VoteService(IMediator mediator) : IVoteService
{
    public Task<Guid> Create(AnswerVoteCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new VotesCreateVoteCommand { Request = dto }, token);
    }
}