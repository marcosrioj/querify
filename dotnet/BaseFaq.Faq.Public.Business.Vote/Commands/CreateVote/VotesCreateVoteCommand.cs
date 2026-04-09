using MediatR;

namespace BaseFaq.Faq.Public.Business.Vote.Commands.CreateVote;

public sealed class VotesCreateVoteCommand : IRequest<Guid>
{
    public required Guid FaqItemAnswerId { get; set; }
}
