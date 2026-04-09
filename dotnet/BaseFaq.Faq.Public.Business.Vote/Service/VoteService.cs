using BaseFaq.Faq.Public.Business.Vote.Abstractions;
using BaseFaq.Faq.Public.Business.Vote.Commands.CreateVote;
using BaseFaq.Models.Faq.Dtos.Vote;
using MediatR;

namespace BaseFaq.Faq.Public.Business.Vote.Service;

public class VoteService(IMediator mediator) : IVoteService
{
    public async Task<Guid> Vote(VoteCreateRequestDto requestDto, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(requestDto);

        return await mediator.Send(new VotesCreateVoteCommand
        {
            FaqItemAnswerId = requestDto.FaqItemAnswerId
        }, token);
    }
}
