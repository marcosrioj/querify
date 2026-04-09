using MediatR;

namespace BaseFaq.Faq.Portal.Business.Vote.Commands.UpdateVote;

public sealed class VotesUpdateVoteCommand : IRequest
{
    public required Guid Id { get; set; }
    public required Guid FaqItemAnswerId { get; set; }
}
