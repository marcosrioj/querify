using MediatR;

namespace BaseFaq.Faq.Portal.Business.Vote.Commands.CreateVote;

public sealed class VotesCreateVoteCommand : IRequest<Guid>
{
    public required Guid FaqItemAnswerId { get; set; }
}
