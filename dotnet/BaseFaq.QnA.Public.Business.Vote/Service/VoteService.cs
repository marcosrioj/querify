using BaseFaq.Models.QnA.Dtos.Answer;
using BaseFaq.QnA.Public.Business.Vote.Abstractions;
using BaseFaq.QnA.Public.Business.Vote.Commands;
using MediatR;

namespace BaseFaq.QnA.Public.Business.Vote.Service;

public sealed class VoteService(IMediator mediator) : IVoteService
{
    public Task<Guid> Create(AnswerVoteCreateRequestDto dto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(dto);
        return mediator.Send(new VotesCreateVoteCommand { Request = dto }, token);
    }
}
