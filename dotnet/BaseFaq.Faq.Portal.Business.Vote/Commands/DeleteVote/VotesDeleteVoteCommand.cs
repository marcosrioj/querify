using MediatR;

namespace BaseFaq.Faq.Portal.Business.Vote.Commands.DeleteVote;

public sealed class VotesDeleteVoteCommand : IRequest
{
    public required Guid Id { get; set; }
}
